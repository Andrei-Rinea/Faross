using System;
using System.Text;
using Faross.Util;

namespace Faross.Models
{
    public class HttpContentCondition : HttpCheckCondition
    {
        public enum Operator
        {
            Undefined,
            Contains,
            DoesNotContain
        }

        [Flags]
        public enum Arguments
        {
            None,
            IgnoreCase
        }

        public HttpContentCondition(
            string name,
            bool stopOnFail,
            Operator op,
            string value,
            Arguments args = Arguments.None,
            Encoding fallbackEncoding = null) : base(name, stopOnFail)
        {
            if (op == default(Operator)) throw new ArgumentOutOfRangeException(nameof(op));
            if (string.IsNullOrEmpty(value)) throw new ArgumentOutOfRangeException(nameof(value));

            Op = op;
            Value = value;
            Args = args;
            FallbackEncoding = fallbackEncoding ?? Encoding.UTF8;
        }

        public Arguments Args { get; }
        public string Value { get; }
        public Operator Op { get; }
        public Encoding FallbackEncoding { get; }

        public override HttpCheckConditionType Type => HttpCheckConditionType.Content;

        public override bool Equals(object obj)
        {
            var other = obj as HttpContentCondition;
            return other != null &&
                   other.StopOnFail == StopOnFail &&
                   other.Args == Args &&
                   other.Op == Op &&
                   other.Value == Value &&
                   Equals(other.FallbackEncoding, FallbackEncoding);
        }

        public override int GetHashCode()
        {
            return HashCodeUtil.GetCombinedHash(Value, Args, Op, StopOnFail);
        }
    }
}