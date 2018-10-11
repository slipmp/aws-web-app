using System;
using System.Collections.Generic;
using System.Text;

namespace Forro.Services
{
    public class CloudWatchLogger : ILoggerManager
    {
        public void LogError(string message)
        {
            System.Diagnostics.Debug.WriteLine("Marcos: " + message);
        }
    }
}
