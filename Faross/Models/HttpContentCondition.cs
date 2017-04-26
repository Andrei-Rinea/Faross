using System;
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

        public HttpContentCondition(bool stopOnFail, Operator op, string value, Arguments args) : base(stopOnFail)
        {
            if (op == default(Operator)) throw new ArgumentOutOfRangeException(nameof(op));
            if (string.IsNullOrEmpty(value)) throw new ArgumentOutOfRangeException(nameof(value));

            Op = op;
            Value = value;
            Args = args;
        }

        public Arguments Args { get; }
        public string Value { get; }
        public Operator Op { get; }

        public override HttpCheckConditionType Type => HttpCheckConditionType.Content;

        public override bool Equals(object obj)
        {
            var other = obj as HttpContentCondition;
            return other != null &&
                   other.StopOnFail == StopOnFail &&
                   other.Args == Args &&
                   other.Op == Op &&
                   other.Value == Value;
        }

        public override int GetHashCode()
        {
            return HashCodeUtil.GetCombinedHash(Value, Args, Op, StopOnFail);
        }
    }
}