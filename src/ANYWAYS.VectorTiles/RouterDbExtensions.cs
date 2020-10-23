using System.Collections.Generic;
using Itinero;
using Itinero.LocalGeo;
using Itinero.VectorTiles;
using Itinero.VectorTiles.Tiles;

namespace Anyways.VectorTiles
{
    public static class RouterDbExtensions
    {
        /// <summary>
        /// Calculates the bounding box for the given router db.
        /// </summary>
        /// <param name="routerDb">The router db.</param>
        /// <returns>The bounding box.</returns>
        public static Box CalculateBoundingBox(this RouterDb routerDb)
        { 
            // TODO: move this to itinero.vectortiles.
            var box = new Box(routerDb.Network.GetVertex(0), 
                routerDb.Network.GetVertex(0));
            for (uint v = 1; v < routerDb.Network.VertexCount; v++)
            {
                var l = routerDb.Network.GetVertex(v);
                box = box.ExpandWith(l.Latitude, l.Longitude);
            }

            return box;
        }
        
        public static IEnumerable<(Tile tile, VectorTile data)> ExtractTiles(this RouterDb routerDb, VectorTileConfig config, int minZoom = 0, int maxZoom = 14)
        {
            // TODO: move this to itinero.vectortiles.
            var bbox = routerDb.CalculateBoundingBox();

            return routerDb.ExtractTiles(config, bbox, minZoom, maxZoom);
        }
        public static IEnumerable<(Tile tile, VectorTile data)> ExtractTiles(this RouterDb routerDb, VectorTileConfig config, Box bbox, int minZoom = 0, int maxZoom = 14)
        {
            // TODO: move this to itinero.vectortiles.
            for (var z = minZoom; z <= maxZoom; z++)
            {
                var topLeftTile = Tile.CreateAroundLocation(bbox.MaxLat, bbox.MinLon, z);
                var bottomRightTile = Tile.CreateAroundLocation(bbox.MinLat, bbox.MaxLon, z);

                for (var x = topLeftTile.X; x <= bottomRightTile.X; x++)
                for (var y = topLeftTile.Y; y <= bottomRightTile.Y; y++)
                {
                    var t = new Tile(x, y, z);

                    yield return (t, routerDb.ExtractTile(t.Id, config));
                }
            }
        }
    }
}