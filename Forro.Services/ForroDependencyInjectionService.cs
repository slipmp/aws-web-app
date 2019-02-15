using Amazon;
using Amazon.DynamoDBv2;
using Amazon.S3;
using Forro.Data;
using Forro.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Forro.Services
{
    public class ForroDependencyInjectionService
    {
        private readonly IForroAppConfig _forroAppConfig;
        public ForroDependencyInjectionService(IForroAppConfig forroAppConfig)
        {
            _forroAppConfig = forroAppConfig;
        }

        public void DeclareDependencies(IServiceCollection services)
        {
            services.AddSingleton<IForroAppConfig>(x =>
            {
                var result = new ForroAppConfig
                {
                    AWSForroBucketName = _forroAppConfig.AWSForroBucketName,
                    AWSRegionEndpoint = _forroAppConfig.AWSRegionEndpoint
                };
                return result;
            });
            
            services.AddScoped<ILoggerManager>(x =>
            {
                var forroAppConfig = x.GetService<IForroAppConfig>();
                var regionObject = RegionEndpoint.GetBySystemName(forroAppConfig.AWSRegionEndpoint);

                var cloudWatchLogger = new CloudWatchLogger(regionObject);
                return cloudWatchLogger;
            });
            services.AddScoped<IForroLevelService>(x =>
            {
                var forroAppConfig = x.GetService<IForroAppConfig>();
                var regionObject = RegionEndpoint.GetBySystemName(forroAppConfig.AWSRegionEndpoint);

                var config = new AmazonDynamoDBConfig
                {
                    RegionEndpoint = regionObject,
                    Timeout = new System.TimeSpan(0, 1, 0)
                };
                var client = new AmazonDynamoDBClient(config);

                var forroLevelRepository = new ForroLevelRepository(client);

                var s3Client = new AmazonS3Client(regionObject);
                var loggerManager = new CloudWatchLogger(regionObject);
                var forroLevelMessage = x.GetRequiredService<IForroLevelMessage>();

                var forroLevelService = new ForroLevelService(forroLevelRepository, s3Client, loggerManager,
                    forroAppConfig.AWSForroBucketName, forroAppConfig.AWSRegionEndpoint, forroLevelMessage);

                return forroLevelService;
            });

            services.AddScoped<IForroLevelMessage>(x =>
            {
                var forroAppConfig = x.GetService<IForroAppConfig>();
                var regionObject = RegionEndpoint.GetBySystemName(forroAppConfig.AWSRegionEndpoint);

                //I will leave it hard-coded - maybe not optimum but suits my immediate needs.
                var forroLevelQueueName = "Paulo-Forro-SQS";

                var forroLevelMessage = new ForroLevelMessage(forroLevelQueueName, regionObject);
                return forroLevelMessage;
            });
        }
    }
}
