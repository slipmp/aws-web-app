using Forro.Domain;
using Forro.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

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

            var forroAppConfig = new ForroAppConfig();
            Configuration.GetSection("ForroAppConfig").Bind(forroAppConfig);
            
            var forroDependencyInjectionService = new ForroDependencyInjectionService(forroAppConfig);
            forroDependencyInjectionService.DeclareDependencies(services);

            var serviceProvider = services.BuildServiceProvider();
            var nLogConfig = new NLogConfig(serviceProvider);
            nLogConfig.ConfigNLogToUseAWSCloudWatch();

            EnsureSQSForroLevelCreation(serviceProvider);
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        private void EnsureSQSForroLevelCreation(IServiceProvider serviceProvider)
        {
            var forroLevelMessage = serviceProvider.GetRequiredService<IForroLevelMessage>();
            forroLevelMessage.EnsureSQSForroLevelCreation();
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
    }
}
