using Itinero;
using Itinero.LocalGeo;

namespace Anyways.VectorTiles.CycleNetworks
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
            var box = new Box(routerDb.Network.GetVertex(0), 
                routerDb.Network.GetVertex(0));
            for (uint v = 1; v < routerDb.Network.VertexCount; v++)
            {
                var l = routerDb.Network.GetVertex(v);
                box = box.ExpandWith(l.Latitude, l.Longitude);
            }

            return box;
        }
    }
}