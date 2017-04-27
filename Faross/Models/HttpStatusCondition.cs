using System;
using Faross.Util;

namespace Faross.Models
{
    public class HttpStatusCondition : HttpCheckCondition
    {
        public enum Operator
        {
            Undefined,
            Equal,
            NotEqual
        }

        public HttpStatusCondition(string name, bool stopOnFail, Operator op, int status) : base(name, stopOnFail)
        {
            if (op == default(Operator)) throw new ArgumentOutOfRangeException(nameof(op));
            if (status < 100 || status > 599) throw new ArgumentOutOfRangeException(nameof(status));

            Op = op;
            Status = status;
        }

        public Operator Op { get; }
        public int Status { get; }

        public override HttpCheckConditionType Type => HttpCheckConditionType.Status;

        public override bool Equals(object obj)
        {
            var other = obj as HttpStatusCondition;
            return other != null &&
                   other.Op == Op &&
                   other.Status == Status;
        }

        public override int GetHashCode()
        {
            return HashCodeUtil.GetCombinedHash(Status, Op, StopOnFail);
        }
    }
}