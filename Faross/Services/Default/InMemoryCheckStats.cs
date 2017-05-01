using System.Linq;
using System.Collections.Generic;
using Faross.Models;
using System;

namespace Faross.Services.Default
{
    public class InMemoryCheckStats : ICheckStats
    {
        private readonly Dictionary<CheckBase, Statistics> _allStats;

        public InMemoryCheckStats()
        {
            _allStats = new Dictionary<CheckBase, Statistics>();
        }

        public void AddCheckResult(CheckResult checkResult)
        {
            Statistics stats;
            var check = checkResult.Check;
            var haveStats = _allStats.TryGetValue(check, out stats);

            if (!haveStats)
            {
                stats = new Statistics(checkResult);
            }
            else
            {
                var previousResult = stats.CurrentResult;
                if (checkResult.SameStatus(previousResult))
                {
                    var @for = checkResult.Time - previousResult.Time;
                    stats = new Statistics(checkResult, @for);
                }
                else
                {
                    stats = new Statistics(checkResult, previousResult);
                }
            }
            _allStats[check] = stats;
        }

        public IEnumerable<Statistics> GetAllStats()
        {
            return _allStats.Select(p => p.Value).ToList().AsReadOnly();
        }

        public Statistics GetStat(CheckBase check)
        {
            if (check == null) throw new ArgumentNullException(nameof(check));
            return _allStats[check];
        }
    }
}