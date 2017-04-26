using System;
using Faross.Models;
using Faross.Util;

namespace Faross.Services.Checkers
{
    public class HttpChecker : IChecker
    {
        private readonly ITimeService _timeService;
        private readonly int _readBufferSize;
        private readonly int _connectSleepMilliseconds;

        public HttpChecker(ITimeService timeService, int readBufferSize = 4096, int connectSleepMilliseconds = 10)
        {
            _timeService = timeService ?? throw new ArgumentNullException(nameof(timeService));

            _readBufferSize = readBufferSize;
            _connectSleepMilliseconds = connectSleepMilliseconds;
        }

        public CheckResult Check(CheckBase checkBase)
        {
            if (checkBase == null) throw new ArgumentNullException(nameof(checkBase));
            var httpCheck = checkBase as HttpCheck;
            if (httpCheck == null) throw new ArgumentException("check is not of type HttpCall");

            var result = HttpUtil.GetContentFromUrl(
                httpCheck.Url,
                httpCheck.ConnectTimeout,
                httpCheck.ReadTimeout,
                _readBufferSize,
                _connectSleepMilliseconds);

            var now = _timeService.UtcNow;

            foreach (var condition in httpCheck.Conditions)
            {
                switch (condition.Type)
                {
                    case HttpCheckConditionType.Status:
                        CheckStatus(result, condition);
                        break;
                    case HttpCheckConditionType.Content:
                        CheckContent(result, condition);
                        break;
                    // ReSharper disable once RedundantCaseLabel
                    case HttpCheckConditionType.Undefined:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(checkBase), condition.Type, "condition has no type");
                }
            }


            throw new NotImplementedException();
        }

        private static string CheckContent(HttpUtil.GetContentResult result, HttpCheckCondition condition)
        {
            throw new NotImplementedException();
        }

        private static string CheckStatus(HttpUtil.GetContentResult result, HttpCheckCondition condition)
        {
            throw new NotImplementedException();

//            switch (condition.Operator)
//            {
//                case HttpCheckCondition.CheckOperator.Equals:
//                    var desiredStatus = int.Parse(condition.Value);
//                    break;
//                case HttpCheckCondition.CheckOperator.DoesNotContain:
//                    throw new InvalidOperationException();
//                // ReSharper disable once RedundantCaseLabel
//                case HttpCheckCondition.CheckOperator.Undefined:
//                default:
//                    throw new ArgumentOutOfRangeException(nameof(condition));
//            }
//            throw new NotImplementedException();
        }
    }
}