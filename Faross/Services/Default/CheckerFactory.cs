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

        public IChecker GetChecker(CheckBase check)
        {
            if (check == null) throw new ArgumentNullException(nameof(check));
            var type = check.GetType();
            if (type == typeof(HttpCheck)) return new HttpChecker(_timeService);
            throw new ArgumentOutOfRangeException($"unknown check type {type}");
        }
    }
}