using System;
using System.Collections.Generic;
using System.Text;

namespace Forro.Services
{
    public interface ILoggerManager
    {
        void LogError(string message);
        void LogInfo(string message);
    }
}
