using System.Collections.Generic;
using System.IO;
using Serilog;
using Itinero;
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
            
            // load routerdb.
            var routerDb = RouterDb.Deserialize(File.OpenRead("belgium.routerdb"));
            
            // write vector tiles.
            var bbox = routerDb.CalculateBoundingBox();
            
            var config = new VectorTileConfig()
            {
                VertexLayerConfigs = new List<VertexLayerConfig>()
                {
                    new VertexLayerConfig()
                    {
                        Name = "cyclenodes",
                        GetAttributesFunc = (vertex) => routerDb.GetVertexAttributes(vertex),
                        GetLocationFunc = (vertex) => routerDb.Network.GetVertex(vertex),
                        IncludeFunc = (vertex) =>
                        {
                            var vertexMeta = routerDb.GetVertexAttributes(vertex);
                            foreach (var a in vertexMeta)
                            {
                                if (a.Key == "rcn_ref")
                                {
                                    return true;
                                }
                            }

                            return false;
                        }
                    }
                }
            };

            
            // loop over all tiles in the bounding box.
            var minZoom = 9;
            var maxZoom = 14;

            for (var z = minZoom; z <= maxZoom; z++)
            {
                var topLeftTile = Tile.CreateAroundLocation(bbox.MaxLat, bbox.MinLon, z);
                var bottomRightTile = Tile.CreateAroundLocation(bbox.MinLat, bbox.MaxLon, z);

                for (var x = topLeftTile.X; x <= bottomRightTile.X; x++)
                for (var y = topLeftTile.Y; y <= bottomRightTile.Y; y++)
                {
                    var t = new Tile(x, y, z);
                    
                    var vectorTile = routerDb.ExtractTile(t.Id, config);
                    if (vectorTile.IsEmpty) continue;
                    
                    Log.Information("Writing tile: {0}", t.ToInvariantString());
                    var fileInfo = new FileInfo(Path.Combine("tiles", t.Zoom.ToString(), t.X.ToString(), $"{t.Y}.mvt"));
                    if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
                    {
                        fileInfo.Directory.Create();
                    }
                    using (var stream = fileInfo.Open(FileMode.Create))
                    {
                        Itinero.VectorTiles.Mapbox.MapboxTileWriter.Write(vectorTile, stream);
                    }
                }
            }
        }
    }
}
