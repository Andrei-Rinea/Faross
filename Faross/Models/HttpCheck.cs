using System;
using System.Collections.Generic;
using System.Linq;
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

        internal const int DefaultMaxContentLength = 65536;

        public HttpCheck(
            long id,
            Environment environment,
            Service service,
            TimeSpan interval,
            Uri url,
            IReadOnlyCollection<HttpCheckCondition> conditions,
            HttpMethod method,
            TimeSpan? connectTimeout = null,
            TimeSpan? readTimeout = null,
            int maxContentLength = DefaultMaxContentLength) : base(id, environment, service, interval, conditions)
        {
            if (method == default(HttpMethod)) throw new ArgumentOutOfRangeException(nameof(method));
            if (interval < TimeSpan.MinValue) throw new ArgumentOutOfRangeException(nameof(interval));
            if (maxContentLength <= 0) throw new ArgumentOutOfRangeException(nameof(maxContentLength));

            Url = url ?? throw new ArgumentNullException(nameof(url));
            Method = method;
            ConnectTimeout = connectTimeout ?? DefaultConnectTimeout;
            ReadTimeout = readTimeout ?? DefaultReadTimeout;
            MaxContentLength = maxContentLength;

            if (Conditions.Any(c => c == null)) throw new ArgumentException("conditions contains a null");
        }

        public override CheckType Type => CheckType.HttpCall;

        public Uri Url { get; }
        public HttpMethod Method { get; }
        public TimeSpan ConnectTimeout { get; }
        public TimeSpan ReadTimeout { get; }
        public int MaxContentLength { get; }

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