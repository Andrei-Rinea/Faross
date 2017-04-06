using System;
using System.Collections.Generic;

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
    }
}