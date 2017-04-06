using Faross.Models;

namespace Faross.Services
{
    public interface ICheckStats
    {
        void AddCheckResult(CheckResult checkResult);
    }
}