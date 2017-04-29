using System;
using Faross.Models;
using Faross.Services.Checkers;

namespace Faross.Services.Default
{
    public class CheckerFactory : ICheckerFactory
    {
        private readonly ITimeService _timeService;

        public CheckerFactory(ITimeService timeService)
        {
            _timeService = timeService ?? throw new ArgumentNullException(nameof(timeService));
        }

        public IChecker GetChecker(CheckType type)
        {
            switch (type)
            {
                case CheckType.Ping:
                    throw new NotImplementedException();
                case CheckType.HttpCall:
                    return new HttpChecker(_timeService);
                // ReSharper disable once RedundantCaseLabel
                case CheckType.Undefined:
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}