using System;
using System.Collections.Generic;
using Faross.Util;

namespace Faross.Models
{
    public class HttpCheck : CheckBase
    {
        public enum HttpMethod
        {
            Undefined,
            Get
        }

        private static readonly TimeSpan DefaultConnectTimeout = TimeSpan.FromSeconds(15);
        private static readonly TimeSpan DefaultReadTimeout = TimeSpan.FromSeconds(35);

        public HttpCheck(
            long id,
            Environment environment,
            Service service,
            TimeSpan interval,
            Uri url,
            IReadOnlyCollection<HttpCheckCondition> conditions,
            HttpMethod method,
            TimeSpan? connectTimeout = null,
            TimeSpan? readTimeout = null) : base(id, environment, service, interval)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            Conditions = conditions ?? throw new ArgumentNullException(nameof(conditions));
            Method = method;
            ConnectTimeout = connectTimeout ?? DefaultConnectTimeout;
            ReadTimeout = readTimeout ?? DefaultReadTimeout;
        }

        public override CheckType Type => CheckType.HttpCall;

        public IReadOnlyCollection<HttpCheckCondition> Conditions { get; }
        public Uri Url { get; }
        public HttpMethod Method { get; }
        public TimeSpan ConnectTimeout { get; }
        public TimeSpan ReadTimeout { get; }

        public override TimeSpan GetMaxDuration()
        {
            return ConnectTimeout + ReadTimeout;
        }

        protected override bool EqualsCore(ModelBase other)
        {
            var otherCheck = other as HttpCheck;
            return otherCheck != null &&
                   otherCheck.Url.Equals(Url) &&
                   otherCheck.Method == Method &&
                   otherCheck.ConnectTimeout == ConnectTimeout &&
                   otherCheck.ReadTimeout == ReadTimeout &&
                   otherCheck.Conditions.Equivalent(Conditions);
        }
    }
}