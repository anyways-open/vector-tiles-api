using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ANYWAYS.VectorTiles.API
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var configurationBuilder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", true, true);
                    
                // get deploy time settings if present.
                var configuration = configurationBuilder.Build();
                var deployTimeSettings = configuration["deploy-time-settings"] ?? "/var/app/config/appsettings.json";
                configurationBuilder = configurationBuilder
                    .AddJsonFile(deployTimeSettings, true, true);

                // get environment variable prefix.
                configuration = configurationBuilder.Build();
                var envVarPrefix = configuration["env-var-prefix"] ?? "CONF_";
                configurationBuilder = configurationBuilder
                    .AddEnvironmentVariables((c) => { c.Prefix = envVarPrefix; });
                
                // build configuration.
                configuration = configurationBuilder.Build();

                // setup logging.
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();
                
                try
                {
                    var data = configuration["data"];
                    
                    var host = WebHost.CreateDefaultBuilder(args)
                        .ConfigureServices((hostContext, services) =>
                        {
                            // add logging.
                            services.AddLogging(b =>
                            {
                                b.AddSerilog();
                            });
                            
                            // add configuration.
                            services.AddSingleton(new StartupConfiguration()
                            {
                                DataPath = data
                            });
                        }).UseStartup<Startup>().Build();
                    
                    // run!
                    await host.RunAsync();
                }
                catch (Exception e)
                {
                    Log.Logger.Fatal(e, "Unhandled exception.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
