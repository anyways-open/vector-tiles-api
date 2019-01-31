namespace Anyways.VectorTiles
{
    public class VectorTileSource
    {
        public int minzoom { get; set; }
        
        public int maxzoom { get; set; }
        
        public double[] bounds { get; set; }
        
        public string name { get; set; }
        
        public string description { get; set; }
        
        public VectorLayer[] vector_layers { get; set; }
    }
}