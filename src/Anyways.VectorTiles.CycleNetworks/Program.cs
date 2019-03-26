using System;
using System.IO;
using Itinero;
using Itinero.Logging;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Anyways.VectorTiles.CycleNetworks
{
    class Program
    {
        static void Main(string[] args)
        {
            InstallLogging();

            if (args.Length != 3)
            {
                Console.WriteLine("Expected argument: routerdb specification.json outputDirectory");
                return;
            }
            
            var routerDbPath = args[0];
            var specPath = args[1];
            var outputDir = args[2];
            
            Log.Information("Reading routerdb...");
            var routerDb = RouterDb.Deserialize(File.OpenRead(routerDbPath));

            Log.Information("Parsing spec");
            JObject j;
            using (var r = new StreamReader(specPath))
            {
                j = JObject.Parse(r.ReadToEnd());
            }
            Log.Information("Writing tiles...");
            routerDb.CreateVectorTiles(j, outputDir);
            Log.Information("All done!");
        }


        private static void InstallLogging()
        {
            Logger.LogAction = (o, level, message, parameters) =>
            {
                if (level == TraceEventType.Verbose.ToString().ToLower())
                {
                    Log.Debug($"[{o}] {level} - {message}");
                }
                else if (level == TraceEventType.Information.ToString().ToLower())
                {
                    Log.Information($"[{o}] {level} - {message}");
                }
                else if (level == TraceEventType.Warning.ToString().ToLower())
                {
                    Log.Warning($"[{o}] {level} - {message}");
                }
                else if (level == TraceEventType.Critical.ToString().ToLower())
                {
                    Log.Fatal($"[{o}] {level} - {message}");
                }
                else if (level == TraceEventType.Error.ToString().ToLower())
                {
                    Log.Error($"[{o}] {level} - {message}");
                }
                else
                {
                    Log.Debug($"[{o}] {level} - {message}");
                }
            };

            // attach logger.
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}