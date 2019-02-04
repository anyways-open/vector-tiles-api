using System.IO;
using Itinero.VectorTiles.Tiles;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Anyways.VectorTiles.API.Controllers
{
    [ApiController]
    public class TilesController : ControllerBase
    {
        [HttpGet("{tileSet}/mvt.json")]
        public IActionResult Get(string tileSet)
        {
            var tileFileInfo = new FileInfo(Path.Combine(Startup.DataPath, tileSet, $"mvt.json"));

            if (!tileFileInfo.Exists) return NotFound();
            
            var mvt = Newtonsoft.Json.JsonConvert.DeserializeObject<VectorTileSource>(
                System.IO.File.ReadAllText(tileFileInfo.FullName));

            var url = $"{this.Request.Scheme}://{this.Request.Host}/{this.Request.PathBase}{tileSet}/{{z}}/{{x}}/{{y}}.mvt";
            mvt.tiles = new[]
            {
                url
            };
                
            return new JsonResult(mvt);

        }

        [HttpGet("{tileSet}/{z}/{x}/{y}.mvt")]
        public IActionResult Get(string tileSet, int z, int x, int y)
        {
            var t = new Tile(x, y, z);
            var tileFileInfo = new FileInfo(Path.Combine(Startup.DataPath, tileSet, t.Zoom.ToString(), t.X.ToString(), $"{t.Y}.mvt"));

            if (tileFileInfo.Exists)
            {
                return PhysicalFile(tileFileInfo.FullName, "application/x-protobuf");
            }

            return NotFound();
        }
    }
}
