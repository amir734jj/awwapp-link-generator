using System;
using App.Controllers;
using App.Dal;
using App.Filters;
using App.Interfaces;
using App.Logic;
using Hangfire;
using Hangfire.MemoryStorage;
using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static App.Utilities.ConnectionStringUtility;

namespace App
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IAwwAppLogic, AwwAppLogic>();

            services.AddScoped<IAwwAppLinkDal, AwwAppLinkDal>();
            
            services.AddScoped<HomeController>();
            
            services.AddMarten(x =>
            {
                x.Connection(ConnectionStringUrlToPgResource(Environment.GetEnvironmentVariable("DATABASE_URL")));
                x.PLV8Enabled = false;
                x.AutoCreateSchemaObjects = AutoCreate.All;
            });

            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMemoryStorage());

            // Add the processing server as IHostedService
            services.AddHangfireServer();

            services.AddRouting();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IAwwAppLogic logic, IAwwAppLinkDal dal, IBackgroundJobClient backgroundJobs)
        {
            dal.Clean();

            backgroundJobs.Schedule(() => logic.CacheLinks(10), TimeSpan.FromMinutes(1));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHangfireServer();

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {               
                DashboardTitle = "Hangfire Jobs",
                Authorization = new[]
                {
                    new  HangfireAuthorizationFilter("admin")
                }
            });

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
