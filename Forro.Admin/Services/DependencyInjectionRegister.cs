using Amazon;
using Amazon.DynamoDBv2;
using Amazon.S3;
using Forro.Data;
using Forro.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Forro.Admin
{
    public class DependencyInjectionRegister
    {
        public IConfiguration Configuration { get; }

        public DependencyInjectionRegister(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void DeclareDependencies(IServiceCollection services)
        {
            services.Configure<ForroAppConfig>(Configuration.GetSection("ForroAppConfig"));
            
            services.AddScoped<ILoggerManager>(x =>
            {
                var forroAppConfig = x.GetService<IOptions<ForroAppConfig>>();
                var regionObject = RegionEndpoint.GetBySystemName(forroAppConfig.Value.AWSRegionEndpoint);

                var cloudWatchLogger = new CloudWatchLogger(regionObject);
                return cloudWatchLogger;
            });
            services.AddScoped<IForroLevelService>(x =>
            {
                var forroAppConfig = x.GetService<IOptions<ForroAppConfig>>();
                var regionObject = RegionEndpoint.GetBySystemName(forroAppConfig.Value.AWSRegionEndpoint);

                var config = new AmazonDynamoDBConfig
                {
                    RegionEndpoint = regionObject,
                    Timeout = new System.TimeSpan(0, 1, 0)
                };
                var client = new AmazonDynamoDBClient(config);

                var forroLevelRepository = new ForroLevelRepository(client);

                var s3Client = new AmazonS3Client(regionObject);
                var loggerManager = new CloudWatchLogger(regionObject);

                var forroLevelService = new ForroLevelService(forroLevelRepository, s3Client, loggerManager,
                    forroAppConfig.Value.AWSForroBucketName, forroAppConfig.Value.AWSRegionEndpoint);

                return forroLevelService;
            });

        }
    }
}
