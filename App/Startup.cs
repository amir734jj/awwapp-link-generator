using System;
using App.Controllers;
using App.Dal;
using App.Interfaces;
using App.Logic;
using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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

            services.AddRouting();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IAwwAppLogic logic, IAwwAppLinkDal dal)
        {
            dal.Clean().Wait();

            logic.GenerateLinks(10, cacheMode: true).Wait();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
