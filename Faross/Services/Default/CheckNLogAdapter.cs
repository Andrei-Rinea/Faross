using System;
using Faross.Models;
using NLog;

namespace Faross.Services.Default
{
    public class CheckNLogAdapter : ICheckLog
    {
        private readonly ILogger _logger;

        public CheckNLogAdapter(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void LogCheck(CheckResult checkResult)
        {
            
        }
    }
}