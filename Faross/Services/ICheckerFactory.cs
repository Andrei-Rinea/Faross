using Faross.Models;

namespace Faross.Services
{
    public interface ICheckerFactory
    {
        IChecker GetChecker(CheckBase check);
    }
}