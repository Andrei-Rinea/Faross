using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.XPath;
using Faross.Models;
using Faross.Util;
using Environment = Faross.Models.Environment;

namespace Faross.Services.Default
{
    public class XmlFileConfigRepo : IConfigRepo
    {
        private readonly IFileService _fileService;
        private readonly string _xmlPath;

        public XmlFileConfigRepo(IFileService fileService, string xmlPath)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _xmlPath = xmlPath ?? throw new ArgumentNullException(nameof(xmlPath));
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Local
        private static List<Environment> GetEnvironments(XPathNavigator xPathNavigator)
        {
            var environments = new List<Environment>();
            var nodes = xPathNavigator.Select("config/environments/environment");
            while (nodes.MoveNext())
            {
                var crt = nodes.Current;
                var name = crt.GetStringAttributeValue("name");
                var id = crt.GetLongAttributeValue("id");

                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidDataException("encountered 'environment' element without name attribute value");
                if (id == null)
                    throw new InvalidDataException("encountered 'environment' element without id attribute value");

                var environment = new Environment(name, id.Value);
                environments.Add(environment);
            }
            if (environments.Count != environments.Select(e => e.Id).Distinct().Count())
                throw new InvalidDataException("duplicate ids found within environments");
            return environments;
        }

        public Configuration GetConfiguration()
        {
            XPathDocument document;
            using (var configStream = _fileService.Open(_xmlPath))
                document = new XPathDocument(configStream);
            var xPathNavigator = document.CreateNavigator();

            var environments = GetEnvironments(xPathNavigator).AsReadOnly();
            var services = GetServices(xPathNavigator, environments).AsReadOnly();
            var checks = GetChecks(xPathNavigator, services, environments).AsReadOnly();

            return new Configuration(environments, services, checks);
        }

        private static List<CheckBase> GetChecks(XPathNavigator xPathNavigator, IReadOnlyCollection<Service> services,
            IReadOnlyCollection<Environment> environments)
        {
            var checks = new List<CheckBase>();
            var nodes = xPathNavigator.Select("config/checks/check");
            while (nodes.MoveNext())
            {
                var crt = nodes.Current;

                var id = crt.GetLongAttributeValue("id");
                var type = crt.GetEnumAttributeValue<CheckType>("type");
                var envRef = crt.GetLongAttributeValue("envRef");
                var serviceRef = crt.GetLongAttributeValue("serviceRef");
                var interval = crt.GetTimeSpanAttributeValue("interval");

                if (id == null) throw new InvalidDataException("encountered check without (valid?) id");
                if (type == null || type.Value == CheckType.Undefined)
                    throw new InvalidDataException("encountered check without (valid?) type");
                if (envRef == null) throw new InvalidDataException("encountered check without (valid?) envRef");
                if (serviceRef == null) throw new InvalidDataException("encountered check without (valid?) serviceRef");
                if (interval == null || interval.Value <= TimeSpan.MinValue)
                    throw new InvalidDataException("encountered check without (valid?) interval");

                var environment = environments.SingleOrDefault(e => e.Id == envRef.Value);
                var service = services.SingleOrDefault(s => s.Id == serviceRef.Value);

                if (environment == null)
                    throw new InvalidDataException("check '" + id + "' refers not found environment");
                if (service == null) throw new InvalidDataException("check '" + id + "' refers not found service");

                var check = CompleteCheckRead(crt, id.Value, type.Value, environment, service, interval.Value);
                checks.Add(check);
            }
            return checks;
        }

        private static HttpCheck CompleteHttpCheck(XPathNavigator crt, long id, Environment environment, Service service, TimeSpan interval)
        {
            var method = crt.GetEnumAttributeValue<HttpCheck.HttpMethod>("method");
            var url = crt.GetUriAttributeValue("url");
            var connectTimeout = crt.GetTimeSpanAttributeValue("connectTimeout");
            var readTimeout = crt.GetTimeSpanAttributeValue("readTimeout");

            if (method == null) throw new InvalidDataException("HttpCall check is missing/has invalid the (HTTP) method");
            if (url == null) throw new InvalidDataException("HttpCall check is missing/has invalid its URL");
            if (connectTimeout + readTimeout > interval) throw new InvalidDataException("the combined timeouts cannot be larger than the check interval");

            var conditions = new List<HttpCheckCondition>();
            var conditionsNode = crt.SelectSingleNode("conditions");
            conditionsNode.MoveToFirstChild();
            do
            {
                if (conditionsNode.NodeType != XPathNodeType.Element) continue;
                var conditionsClone = conditionsNode.Clone();
                var conditionType = conditionsClone.Name;
                var attributeStrings = new List<KeyValuePair<string, string>>();
                var value = conditionsClone.Value;
                if (conditionsClone.HasAttributes)
                {
                    if (conditionsClone.MoveToFirstAttribute())
                    {
                        do
                        {
                            attributeStrings.Add(new KeyValuePair<string, string>(conditionsClone.Name, conditionsClone.Value));
                        } while (conditionsClone.MoveToNextAttribute());
                    }
                }
                var condition = GetCondition(conditionType, attributeStrings, value);
                conditions.Add(condition);
            } while (conditionsNode.MoveToNext());
            var roConditions = conditions.AsReadOnly();
            var check = new HttpCheck(id, environment, service, interval, url, roConditions, method.Value, connectTimeout, readTimeout);
            return check;
        }

        private static T? GetEnumValue<T>(IEnumerable<KeyValuePair<string, string>> attributes, string attributeName) where T : struct
        {
            var attribute = attributes.SingleOrDefault(a => a.Key == attributeName);
            if (Equals(attribute, default(KeyValuePair<string, string>))) return null;
            var value = attribute.Value;
            T result;
            if (!Enum.TryParse(value, true, out result)) return null;
            return result;
        }

        private static HttpCheckCondition GetCondition(
            string conditionType,
            IReadOnlyCollection<KeyValuePair<string, string>> attributes,
            string value)
        {
            var stopOnFail = attributes.Any(p => p.Key == "stopOnFail" && string.Equals(p.Value, "true", StringComparison.OrdinalIgnoreCase));
            switch (conditionType)
            {
                case "statusCheck":
                {
                    var op = GetEnumValue<HttpStatusCondition.Operator>(attributes, "operator");
                    if (op == null) throw new InvalidDataException("HTTP status condition with invalid/missing operator value");
                    int status;
                    if (!int.TryParse(value, out status)) throw new InvalidDataException("HTTP status condition with invalid/missing status value");
                    return new HttpStatusCondition(stopOnFail, op.Value, status);
                }
                case "contentCheck":
                {
                    var op = GetEnumValue<HttpContentCondition.Operator>(attributes, "operator");
                    if (op == null) throw new InvalidDataException("HTTP content check with missing/invalid operator value");
                    var args = GetEnumValue<HttpContentCondition.Arguments>(attributes, "arguments") ?? HttpContentCondition.Arguments.None;
                    if (value == null) throw new InvalidDataException("HTTP content check with missing value");
                    return new HttpContentCondition(stopOnFail, op.Value, value, args);
                }
                default: throw new InvalidDataException("unknown HTTP condition '" + conditionType + "'");
            }
        }

        private static CheckBase CompleteCheckRead(XPathNavigator crt, long id, CheckType type, Environment environment,
            Service service, TimeSpan interval)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (type)
            {
                case CheckType.Ping:
                    throw new NotImplementedException();
                case CheckType.HttpCall:
                    return CompleteHttpCheck(crt, id, environment, service, interval);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static List<Service> GetServices(XPathNavigator xPathNavigator,
            IReadOnlyCollection<Environment> environments)
        {
            var services = new List<Service>();
            var nodes = xPathNavigator.Select("config/services/service");
            while (nodes.MoveNext())
            {
                var crt = nodes.Current;
                var name = crt.GetStringAttributeValue("name");
                var id = crt.GetLongAttributeValue("id");

                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidDataException("encountered 'service' element without name attribute value");
                if (id == null)
                    throw new InvalidDataException("encountered 'service' element without id attribute value");

                var subNodes = crt.Select("runsOn/envRef");
                var envRefs = new List<Environment>();
                while (subNodes.MoveNext())
                {
                    var crtSub = subNodes.Current;
                    var envRefId = crtSub.GetLongAttributeValue("id");
                    if (envRefId == null)
                        throw new InvalidDataException("service with id '" + id + "' has environment reference with no id");
                    var environment = environments.SingleOrDefault(e => e.Id == envRefId.Value);
                    if (environment == null)
                        throw new InvalidDataException("service with id '" + id + "' references non-defined environment with id '" + envRefId.Value + "'");
                    envRefs.Add(environment);
                }
                if (envRefs.Count == 0)
                    throw new InvalidDataException("service with id '" + id + "' references no environment");
                var service = new Service(id.Value, name, envRefs.AsReadOnly());
                services.Add(service);
            }
            if (services.Count != services.Select(s => s.Id).Distinct().Count())
                throw new InvalidDataException("duplicate ids found within services");
            return services;
        }
    }
}