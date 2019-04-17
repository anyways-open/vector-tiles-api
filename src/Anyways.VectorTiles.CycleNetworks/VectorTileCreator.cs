using System.IO;
using Itinero;
using Itinero.LocalGeo;
using Itinero.VectorTiles;
using Itinero.VectorTiles.Mapbox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Tile = Itinero.VectorTiles.Tiles.Tile;

// ReSharper disable PossibleMultipleEnumeration

namespace Anyways.VectorTiles.CycleNetworks
{
    /// <summary>
    /// Creates all needed vector tiles based on the given specification-json
    /// </summary>
    public static class VectorTileCreator
    {
        public static void CreateVectorTiles(this RouterDb routerDb, JObject specification, string outputDirectory)
        {
            var minZoom = int.Parse(specification.GetFlatValue("minzoom"));
            var maxZoom = int.Parse(specification.GetFlatValue("maxzoom"));

            var config = routerDb.CreateVectorTileConfigFor(specification);


            var bbox = routerDb.CalculateBoundingBox();

            Log.Information($"Lat: {bbox.MinLat} --> {bbox.MaxLat}, Lon: {bbox.MinLon} --> {bbox.MaxLon}");

            // A bounding box containing the outline of all the vector tiles
            Box? box = null;


            for (var z = minZoom; z <= maxZoom; z++)
            {
                var minTile = Tile.CreateAroundLocation(bbox.MinLat, bbox.MinLon, z);
                var maxTile = Tile.CreateAroundLocation(bbox.MaxLat, bbox.MaxLon, z);
                Log.Information(
                    $"Creating tiles for zoomlevel {z} ({minZoom} --> {maxZoom}). x-ranges are {minTile.X} --> {maxTile.X}; y-ranges are {minTile.Y} --> {maxTile.Y}");

                for (var x = minTile.X; x <= maxTile.X; x++)
                {
                    for (var y = minTile.Y; y <= maxTile.Y; y++)
                    {
                        var t = new Tile(x, y, z);

                        Log.Information(
                            $"Generating tile X: {1 + x - minTile.X}/{1 + maxTile.X - minTile.X};" +
                            $" {1 + y - minTile.Y}/{1 + maxTile.Y - minTile.Y} (zoom {z} out of {maxZoom})");
                        var data = routerDb.ExtractTile(t.Id, config);
                        if (data.IsEmpty)
                        {
                            Log.Information("Tile is empty");
                            continue;
                        }


                        CreateAndWriteTile(t, data, outputDirectory);
                        box = box?.ExpandWith(t) ?? t.Box();
                    }
                }

                if (box == null)
                {
                    // No data written at all. Fall out of the loop
                    break;
                }
            }

            if (box == null)
            {
                // no data was written.
                Log.Warning("No data written! Perhaps filtering was to strict?");
                return;
            }

            var vectorTileSource = specification.CreateVectorSourceFor(minZoom, maxZoom, box.Value);

            File.WriteAllText(Path.Combine(outputDirectory, "mvt.json"),
                JsonConvert.SerializeObject(vectorTileSource));
        }

        private static void CreateAndWriteTile(Tile tile, VectorTile data, string outputDirectory)
        {
            var dirPath = Path.Combine(outputDirectory, tile.Zoom.ToString(), tile.X.ToString());
            Directory.CreateDirectory(dirPath); // Does not create if already exists

            var fileInfo = new FileInfo(Path.Combine(dirPath, $"{tile.Y}.mvt"));

            Log.Information($"Writing tile {tile.ToInvariantString()}");
            using (var stream = fileInfo.Open(FileMode.Create))
            {
                data.Write(stream);
            }
        }


        private static Box Box(this Tile tile)
        {
            return new Box(tile.Top, tile.Left, tile.Bottom, tile.Right);
        }

        private static Box ExpandWith(this Box box, Tile tile)
        {
            var tileBox = tile.Box();

            return box.ExpandWith(tileBox);
        }

        private static Box ExpandWith(this Box box, Box other)
        {
            return box.ExpandWith(other.MaxLat, other.MaxLon).ExpandWith(other.MinLat, other.MinLon);
        }
    }
}