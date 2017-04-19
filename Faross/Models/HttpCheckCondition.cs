using System;
using Faross.Util;

namespace Faross.Models
{
    public class HttpCheckCondition
    {
        public enum CheckOperator
        {
            Undefined,
            Equals,
            DoesNotContain
        }

        [Flags]
        public enum CheckArguments
        {
            None = 0,
            IgnoreCase = 1
        }

        public HttpCheckCondition(
            HttpCheckConditionType type,
            CheckOperator @operator,
            CheckArguments arguments,
            string value,
            bool stopOnFail)
        {
            Type = type;
            Operator = @operator;
            Arguments = arguments;
            Value = value;
            StopOnFail = stopOnFail;
        }

        public HttpCheckConditionType Type { get; }
        public CheckOperator Operator { get; }
        public CheckArguments Arguments { get; }
        public string Value { get; }
        public bool StopOnFail { get; }

        public override bool Equals(object obj)
        {
            var other = obj as HttpCheckCondition;
            return other != null &&
                   other.Type == Type &&
                   other.Operator == Operator &&
                   other.Arguments == Arguments &&
                   other.Value == Value &&
                   other.StopOnFail == StopOnFail;
        }

        public override int GetHashCode()
        {
            return HashCodeUtil.GetCombinedHash(Type, Operator, Arguments, Value, StopOnFail);
        }
    }
}