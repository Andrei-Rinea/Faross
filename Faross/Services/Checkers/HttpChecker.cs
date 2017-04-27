using System;
using System.Collections.Generic;
using System.Linq;
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
                _connectSleepMilliseconds,
                httpCheck.MaxContentLength);

            var now = _timeService.UtcNow;
            var conditionResults = new List<ConditionResultDetail>();

            foreach (var conditionBase in httpCheck.Conditions)
            {
                var condition = (HttpCheckCondition) conditionBase;

                string checkMessage;
                switch (condition.Type)
                {
                    case HttpCheckConditionType.Status:
                        checkMessage = CheckStatus(result, (HttpStatusCondition) condition);
                        break;
                    case HttpCheckConditionType.Content:
                        checkMessage = CheckContent(result, (HttpContentCondition) condition);
                        break;
                    // ReSharper disable once RedundantCaseLabel
                    case HttpCheckConditionType.Undefined:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(checkBase), condition.Type, "condition has no type");
                }

                var conditionResult = new ConditionResultDetail(condition.Name, string.IsNullOrEmpty(checkMessage), checkMessage);
                conditionResults.Add(conditionResult);
                if (!conditionResult.Success && condition.StopOnFail)
                    break;
            }

            var outcome = conditionResults.All(c => c.Success) ? CheckOutcome.Success : CheckOutcome.Fail;
            return new CheckResult(checkBase, now, outcome, conditionResults);
        }

        private static string CheckContent(HttpUtil.GetContentResult result, HttpContentCondition condition)
        {
            var encoding = result.GetEncoding();
            var contentString = encoding.GetString(result.Content);
            var value = condition.Value;
            var casing = condition.Args.HasFlag(HttpContentCondition.Arguments.IgnoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            switch (condition.Op)
            {
                case HttpContentCondition.Operator.Contains:
                    return contentString.IndexOf(value, casing) == -1 ? "desired string '" + value + "' was not found in the content" : "";
                case HttpContentCondition.Operator.DoesNotContain:
                    return contentString.IndexOf(value, casing) == -1 ? "" : "undesired string '" + value + "' was found in the content";
                // ReSharper disable once RedundantCaseLabel
                case HttpContentCondition.Operator.Undefined:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string CheckStatus(HttpUtil.GetContentResult result, HttpStatusCondition condition)
        {
            if (!result.Status.HasValue) return "No HTTP status available";
            switch (condition.Op)
            {
                case HttpStatusCondition.Operator.Equal:
                    return result.Status.Value == condition.Status ? "" : "Status (" + result.Status.Value + ") is not equal to desired " + condition.Status + " status";
                case HttpStatusCondition.Operator.NotEqual:
                    return result.Status.Value != condition.Status ? "" : "Status (" + result.Status.Value + ") is equal to undesired " + condition.Status + " status";
                // ReSharper disable once RedundantCaseLabel
                case HttpStatusCondition.Operator.Undefined:
                default:
                    throw new ArgumentOutOfRangeException(nameof(condition));
            }
        }
    }
}