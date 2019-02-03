using Amazon;
using Amazon.DynamoDBv2;
using Amazon.S3;
using Forro.Data;
using Forro.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Forro.Admin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            DeclareDependencies(services);

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerManager logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.ConfigureExceptionHandler(logger);
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }

        public void DeclareDependencies(IServiceCollection services)
        {
            services.Configure<ForroAppConfig>(Configuration.GetSection("ForroAppConfig"));

            services.AddScoped<ILoggerManager, CloudWatchLogger>();
            services.AddScoped<IForroLevelService>(x =>
            {
                var forroAppConfig = x.GetService<IOptions<ForroAppConfig>>();
                var regionObject = RegionEndpoint.GetBySystemName(forroAppConfig.Value.AWSRegionEndpoint);

                var config = new AmazonDynamoDBConfig
                {
                    RegionEndpoint = regionObject
                };
                var client = new AmazonDynamoDBClient(config);

                var forroLevelRepository = new ForroLevelRepository(client);

                var s3Client = new AmazonS3Client(regionObject);
                var forroLevelService = new ForroLevelService(forroLevelRepository, s3Client, 
                    forroAppConfig.Value.AWSForroBucketName, forroAppConfig.Value.AWSRegionEndpoint);

                return forroLevelService;
            });
              
        }
    }
}
