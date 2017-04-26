using System;
using Faross.Models;
using Faross.Services.Checkers;

namespace Faross.Services.Default
{
    public class CheckerFactory : ICheckerFactory
    {
        public IChecker GetChecker(CheckType type)
        {
            switch (type)
            {
                case CheckType.Ping:
                    throw new NotImplementedException();
                case CheckType.HttpCall:
                    //return new HttpChecker();
                    throw new NotImplementedException();
                // ReSharper disable once RedundantCaseLabel
                case CheckType.Undefined:
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}