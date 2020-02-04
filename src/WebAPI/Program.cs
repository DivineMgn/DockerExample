using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;
using System.IO;

namespace WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            { 
                logger.Info("Starting up application ...");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while starting up application.");
                throw;
            }
            finally
            {
                // flush logs to targets before exit
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseIISIntegration()
                        .UseKestrel()
                        .ConfigureAppConfiguration((context, config) => {
                            var env = context.HostingEnvironment;
                            config.SetBasePath(env.ContentRootPath);

                            config
                                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

                            config.AddEnvironmentVariables();

                            // secrets
                            if (env.IsDevelopment())
                            {
                                config.AddUserSecrets<Startup>(optional: true);
                            }

                            if (args != null)
                                config.AddCommandLine(args);
                        })
                        .ConfigureLogging((hostingContext, logging) => {
                            logging.ClearProviders();

                            logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                            logging.AddConsole();
                            logging.AddDebug();
                        })
                        .UseNLog()
                        .UseStartup<Startup>();
                });
    }
}
