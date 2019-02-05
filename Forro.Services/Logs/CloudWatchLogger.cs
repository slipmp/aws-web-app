using System;
using System.Collections.Generic;
using System.Text;

namespace Forro.Services
{
    public class CloudWatchLogger : ILoggerManager
    {
        private readonly Amazon.RegionEndpoint _endpoint;

        public CloudWatchLogger(Amazon.RegionEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public void LogError(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Error(message);
        }

        public void LogInfo(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}
