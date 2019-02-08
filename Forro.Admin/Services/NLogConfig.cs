using Microsoft.Extensions.Options;
using NLog;
using NLog.AWS.Logger;
using NLog.Config;
using System;
using Microsoft.Extensions.DependencyInjection;
using Forro.Domain;

namespace Forro.Admin
{
    public class NLogConfig
    {
        public readonly string _aWSRegionEndpoint;

        public NLogConfig(IServiceProvider serviceProvider)
        {
            var forroAppConfig = serviceProvider.GetRequiredService<IOptions<ForroAppConfig>>();
            _aWSRegionEndpoint = forroAppConfig.Value.AWSRegionEndpoint;
        }

        public void ConfigNLogToUseAWSCloudWatch()
        {
            var config = new LoggingConfiguration();

            //I would have preferred to have this configured on NLog.config
            //However, Region is required and I don't want to duplicated
            //Reason why it is implemented here - NO DUPLICATIONS!! lol
            var awsTarget = new AWSTarget()
            {
                LogGroup = "Paulo-Forro-GroupLogs",
                Region = _aWSRegionEndpoint
            };
            config.AddTarget("aws", awsTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, awsTarget));

            //Write other logs destinations as well
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "file.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;
        }
    }
}
