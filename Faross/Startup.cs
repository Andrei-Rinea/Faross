using System;
using System.Net;
using Faross.Services;
using Faross.Services.Default;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;

namespace Faross
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            env.ConfigureNLog("NLog.config");
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            var mainLogger = NLog.LogManager.GetLogger("Main");

            try
            {
                var mainLog = new NLogAdapter(mainLogger);
                services.AddSingleton<ILog>(mainLog);

                var checkLogger = NLog.LogManager.GetLogger("CheckLog");
                var checkLog = new CheckNLogAdapter(checkLogger);
                services.AddSingleton<ICheckLog>(checkLog);

                var xmlFileConfigRepo = new XmlFileConfigRepo(new FileService(), "Faross.config.xml");
                var config = xmlFileConfigRepo.GetConfiguration();
                services.AddSingleton<IConfigRepo>(xmlFileConfigRepo);

                var timeService = new TimeService();
                services.AddSingleton<ITimeService>(timeService);

                var checkerFactory = new CheckerFactory(timeService);
                var checkStats = new InMemoryCheckStats();
                var scheduler = new ThreadedCheckScheduler(checkLog, checkStats, checkerFactory, mainLog);
                services.AddSingleton<ICheckScheduler>(scheduler);
                services.AddSingleton<ICheckStats>(checkStats);

                scheduler.Init(config);
            }
            catch (Exception ex)
            {
                mainLogger.Fatal(ex.ToString());
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            loggerFactory.AddNLog();
            app.AddNLogWeb();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler(options =>
                {
                    options.Run(
                        async context =>
                        {
                            context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                            context.Response.ContentType = "text/html";
                            var ex = context.Features.Get<IExceptionHandlerFeature>();
                            if (ex != null)
                            {
                                var service = (ILog)app.ApplicationServices.GetService(typeof(ILog));
                                service?.Error(ex.ToString());

                                const string err = "Internal Server Error";
                                await context.Response.WriteAsync(err).ConfigureAwait(false);
                            }
                        });
                });
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}