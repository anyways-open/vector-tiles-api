using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Itinero;
using Itinero.LocalGeo;
using Itinero.Logging;
using Itinero.VectorTiles;
using Itinero.VectorTiles.Layers;
using Itinero.VectorTiles.Tiles;

namespace Anyways.VectorTiles.CycleNetworks
{
    class Program
    {
        static void Main(string[] args)
        {
            Itinero.Logging.Logger.LogAction = (o, level, message, parameters) =>
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
            
            // get parameters.
            var outputPath = "cyclenetworks";

            // load routerdb.
            var routerDb = RouterDb.Deserialize(File.OpenRead("belgium.routerdb"));

            // build configuration.
            const string cycleNodeNetworkId = "cyclenetwork";
            const string cycleNodeId = "cyclenodes";
            var config = new VectorTileConfig()
            {
                EdgeLayerConfigs = new List<EdgeLayerConfig>()
                { 
                    new EdgeLayerConfig()
                    {
                        Name = cycleNodeNetworkId,
                        GetAttributesFunc = (edgeId, zoom) =>
                        {
                            var attributes = routerDb.GetEdgeAttributes(edgeId);
                            if (attributes == null) return null;
                                    
                            var ok = attributes.Any(a => a.Key == "cyclenetwork");
                            if (!ok)
                            {
                                return null;
                            }
                            return attributes;
                        }
                    }
                },
                VertexLayerConfigs = new List<VertexLayerConfig>()
                {
                    new VertexLayerConfig()
                    {
                        Name = cycleNodeId,
                        GetAttributesFunc = (vertex, zoom) =>
                        {
                            var attributes = routerDb.GetVertexAttributes(vertex);
                            var ok = attributes.Any(a => a.Key == "rcn_ref");
                            if (!ok)
                            {
                                return null;
                            }

                            return attributes;
                        }
                    }
                }
            };

            // loop over all tiles in the bounding box.
            var minZoom = 6;
            var maxZoom = 14;
            var vectorTiles = routerDb.ExtractTiles(config, minZoom, maxZoom);
            Box? box = null;
            //Parallel.ForEach(vectorTiles, (vectorTile) =>
            foreach (var vectorTile in vectorTiles)
            {
                if (vectorTile.data.IsEmpty) continue;

                var t = vectorTile.tile;

                Log.Information("Writing tile: {0}", t.ToInvariantString());
                var fileInfo = new FileInfo(Path.Combine(outputPath, t.Zoom.ToString(), t.X.ToString(), $"{t.Y}.mvt"));
                if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }

                using (var stream = fileInfo.Open(FileMode.Create))
                {
                    Itinero.VectorTiles.Mapbox.MapboxTileWriter.Write(vectorTile.data, stream);
                }

                if (box == null)
                {
                    box = vectorTile.tile.Box();
                }
                else
                {
                    box = box.Value.ExpandWith(vectorTile.tile);
                }
            };

            if (!box.HasValue)
            { // no data was written.
                return;
            }
            
            // build meta.
            var vectorTileSource = new VectorTileSource()
            {
                id="cyclenetworks",
                basename = "cyclenetworks",
                name = "cyclenetworks",
                bounds = new double[]{box.Value.MinLon, box.Value.MinLat, box.Value.MaxLon, box.Value.MaxLat},
                maxzoom = maxZoom,
                minzoom = minZoom,
                description = "All info about the cycling node networks.",
                vector_layers = new []
                {
                    new VectorLayer()
                    {
                        maxzoom = maxZoom,
                        minzoom = minZoom,
                        description = "The node-based cycle network",
                        id = cycleNodeNetworkId
                    },
                    new VectorLayer()
                    {
                        maxzoom = maxZoom,
                        minzoom = minZoom,
                        description = "The nodes in the cycle network",
                        id = cycleNodeId
                    }
                }
            };
            File.WriteAllText(Path.Combine(outputPath, "mvt.json"), Newtonsoft.Json.JsonConvert.SerializeObject(vectorTileSource));
        }
    }
}
