using System;
using System.Collections.Generic;

namespace Faross.Models
{
    public class HttpCheck : CheckBase
    {
        public enum HttpMethod
        {
            Undefined,
            Get
        }

        public HttpCheck(
            long id,
            Environment environment,
            Service service,
            TimeSpan interval,
            Uri url,
            IReadOnlyCollection<HttpCheckCondition> conditions,
            HttpMethod method) : base(id, environment, service, interval)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            Conditions = conditions ?? throw new ArgumentNullException(nameof(conditions));
            Method = method;
        }

        public override CheckType Type => CheckType.HttpCall;

        public IReadOnlyCollection<HttpCheckCondition> Conditions { get; }
        public Uri Url { get; }
        public HttpMethod Method { get; }
    }
}