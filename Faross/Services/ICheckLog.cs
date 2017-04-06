using Faross.Models;

namespace Faross.Services
{
    public interface ICheckLog
    {
        void LogCheck(CheckResult checkResult);
    }
}