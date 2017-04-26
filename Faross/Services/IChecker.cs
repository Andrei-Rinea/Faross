using Faross.Models;

namespace Faross.Services
{
    public interface IChecker
    {
        CheckResult Check(CheckBase check);
    }
}