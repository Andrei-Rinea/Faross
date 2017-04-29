using System;
using NLog;

namespace Faross.Services.Default
{
    public class NLogAdapter : ILog
    {
        private readonly ILogger _logger;

        public NLogAdapter(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Info(string message, params object[] arguments)
        {
            _logger.Info(message, arguments);
        }

        public void Trace(string message, params object[] arguments)
        {
            _logger.Trace(message, arguments);
        }

        public void Debug(string message, params object[] arguments)
        {
            _logger.Debug(message, arguments);
        }

        public void Warn(string message, params object[] arguments)
        {
            _logger.Warn(message, arguments);
        }

        public void Error(string message, params object[] arguments)
        {
            _logger.Error(message, arguments);
        }

        public void Fatal(string message, params object[] arguments)
        {
            _logger.Fatal(message, arguments);
        }
    }
}