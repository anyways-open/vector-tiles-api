namespace Anyways.VectorTiles
{
    public class VectorTileSource
    {
        public string[] tiles { get; set; } = {"https://localhost:5001/cyclenetworks/{z}/{x}/{y}.mvt"};
        
        public int minzoom { get; set; }
        
        public int maxzoom { get; set; }
        
        public double[] bounds { get; set; }
        
        public string name { get; set; }
        
        public string description { get; set; }

        public string attribution { get; set; } =
            "<a href=\"http://www.openstreetmap.org/about/\" target=\"_blank\">&copy; OpenStreetMap contributors</a>";

        public string format { get; set; } = "pbf";
        
        public string id { get; set; }
        
        public string basename { get; set; }
        
        public VectorLayer[] vector_layers { get; set; }

        public string version { get; set; } = "1.0";
        
        public string tilejson { get; set; } = "2.0.0";
    } 
}