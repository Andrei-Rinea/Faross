using System;

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
    }
}