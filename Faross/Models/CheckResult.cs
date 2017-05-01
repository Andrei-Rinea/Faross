using System;
using System.Collections.Generic;
using System.Linq;
using Faross.Util;

namespace Faross.Models
{
    public class CheckResult
    {
        public CheckResult(
            CheckBase check,
            DateTimeOffset time,
            CheckOutcome outcome,
            IReadOnlyCollection<ConditionResultDetail> details)
        {
            if (outcome == default(CheckOutcome)) throw new ArgumentOutOfRangeException(nameof(outcome));

            Check = check ?? throw new ArgumentNullException(nameof(check));
            Time = time;
            Outcome = outcome;
            Details = details ?? throw new ArgumentNullException(nameof(details));

            if (Details.Any(d => d == null)) throw new ArgumentException("details contains a null");
        }

        public CheckBase Check { get; }
        public DateTimeOffset Time { get; }
        public CheckOutcome Outcome { get; }
        public IReadOnlyCollection<ConditionResultDetail> Details { get; }

        public override bool Equals(object obj)
        {
            var otherRes = obj as CheckResult;
            return otherRes != null &&
                   otherRes.Check.Equals(Check) &&
                   otherRes.Time == Time &&
                   otherRes.Outcome == Outcome &&
                   otherRes.Details.Equivalent(Details);
        }

        public override int GetHashCode()
        {
            return HashCodeUtil.GetCombinedHash(Check, Time, Outcome, Details);
        }

        public bool SameStatus(CheckResult other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (ReferenceEquals(this, other)) return true;
            return other.Check.Equals(Check) &&
                   other.Outcome == Outcome &&
                   other.Details.Equivalent(Details);
        }
    }
}