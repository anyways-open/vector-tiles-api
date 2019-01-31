using Itinero.LocalGeo;
using Itinero.VectorTiles.Tiles;

namespace Anyways.VectorTiles.CycleNetworks
{
    public static class TileExtensions
    {
        public static Box Box(this Tile tile)
        {
            return new Box(tile.Top, tile.Left, tile.Bottom, tile.Right);
        }

        public static Box ExpandWith(this Box box, Tile tile)
        {
            var tileBox = tile.Box();

            return box.ExpandWith(tileBox);
        }

        public static Box ExpandWith(this Box box, Box other)
        {
            return box.ExpandWith(other.MaxLat, other.MaxLon).ExpandWith(other.MinLat, other.MinLon);
        }
    }
}