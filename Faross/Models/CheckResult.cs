using System;
using System.Collections.Generic;
using Faross.Util;

namespace Faross.Models
{
    public class CheckResult
    {
        public CheckResult(
            CheckBase check,
            DateTimeOffset time,
            CheckOutcome outcome,
            IReadOnlyCollection<CheckResultDetail> details)
        {
            Check = check ?? throw new ArgumentNullException(nameof(check));
            Time = time;
            Outcome = outcome;
            Details = details;
        }

        public CheckBase Check { get; }
        public DateTimeOffset Time { get; }
        public CheckOutcome Outcome { get; }
        public IReadOnlyCollection<CheckResultDetail> Details { get; }

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
    }
}