using System.Globalization;
using System.Linq;

namespace ANYWAYS.VectorTiles.API.MBTiles
{
    internal static class MetadataExtensions
    {
        public static VectorTileSource ToVectorTileSource(this Metadata metadata, string id)
        {
            var json = Newtonsoft.Json.JsonConvert.DeserializeObject<VectorTileSource>(metadata.json);
            var bbox = metadata.bounds.Split(",").Select(x =>
                double.Parse(x, NumberStyles.Any, CultureInfo.InvariantCulture)).ToArray();
            var minzoom = int.Parse(metadata.minzoom);
            var maxzoom = int.Parse(metadata.maxzoom);

            return new VectorTileSource()
            {
                id = id,
                basename = id,
                type = metadata.type,
                name = metadata.name,
                description = metadata.description,
                minzoom = minzoom,
                maxzoom = maxzoom,
                bounds = bbox,
                vector_layers = json.vector_layers
            };
        }
    }
}