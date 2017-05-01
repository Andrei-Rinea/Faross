using System.Collections.Generic;
using Faross.Models;

namespace Faross.Services
{
    public interface ICheckStats
    {
        void AddCheckResult(CheckResult checkResult);
        IEnumerable<Statistics> GetAllStats();
        Statistics GetStat(CheckBase check);
    }
}