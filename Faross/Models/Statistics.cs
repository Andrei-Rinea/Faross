using System;

namespace Faross.Models
{
    public class Statistics
    {
        public enum Variation
        {
            Undefined,
            NoData,
            SameOutcomeDifferentDetails,
            DifferentOutcome
        }

        public Statistics(CheckResult currentResult, TimeSpan? since = null)
        {
            if (ChangeSince <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(since));
            CurrentResult = currentResult ?? throw new ArgumentNullException(nameof(currentResult));
            PreviousDifferentResult = null;
            ChangeSince = since;
            ChangeType = Variation.NoData;
        }

        public Statistics(CheckResult currentResult, CheckResult previousDifferentResult)
        {
            CurrentResult = currentResult ?? throw new ArgumentNullException(nameof(currentResult));
            PreviousDifferentResult = previousDifferentResult ?? throw new ArgumentNullException(nameof(previousDifferentResult));
            if (currentResult.Time <= previousDifferentResult.Time)
                throw new ArgumentException($"previous different result is after current: {currentResult.Time} vs {previousDifferentResult.Time}");
            if (!Equals(currentResult.Check, previousDifferentResult.Check))
                throw new ArgumentException($"results from different checks: {currentResult.Check} vs {previousDifferentResult.Check}");

            ChangeType = currentResult.Outcome == previousDifferentResult.Outcome ? Variation.SameOutcomeDifferentDetails : Variation.DifferentOutcome;
            ChangeSince = currentResult.Time - previousDifferentResult.Time;
        }

        public CheckResult CurrentResult { get; }
        public CheckResult PreviousDifferentResult { get; }
        public Variation ChangeType { get; }
        public TimeSpan? ChangeSince { get; }

        public CheckBase Check => CurrentResult.Check;
    }
}