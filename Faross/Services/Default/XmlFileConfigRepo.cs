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

            var sub = crt.Select("conditions/httpCheckCondition");
            var conditions = new List<HttpCheckCondition>();
            while (sub.MoveNext())
            {
                var crtSub = sub.Current;
                var conditionType = crtSub.GetEnumAttributeValue<HttpCheckConditionType>("type");
                var @operator = crtSub.GetEnumAttributeValue<HttpCheckCondition.CheckOperator>("operator");
                if (conditionType == null) throw new InvalidDataException("httpCheckCondition has missing/invalid type");
                if (@operator == null) throw new InvalidDataException("httpCheckCondition has missing/invalid operator");

                var arguments = crtSub.GetEnumAttributeValue<HttpCheckCondition.CheckArguments>("arguments") ?? HttpCheckCondition.CheckArguments.None;
                var value = crtSub.Value;

                var stopOnFail = crtSub.GetBoolAttributeValue("stopOnFail") ?? false;
                var condition = new HttpCheckCondition(conditionType.Value, @operator.Value, arguments, value, stopOnFail);
                conditions.Add(condition);
            }
            if (conditions.Count == 0) throw new InvalidDataException("HttpCall check has no conditions");
            var roConditions = conditions.AsReadOnly();
            var check = new HttpCheck(id, environment, service, interval, url, roConditions, method.Value, connectTimeout, readTimeout);
            return check;
        }

        private static CheckBase CompleteCheckRead(XPathNavigator crt, long id, CheckType type, Environment environment,
            Service service, TimeSpan interval)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (type)
            {
                case CheckType.Ping:
                    throw new NotImplementedException();
                case CheckType.HttpConnect:
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