using System.IO;
using Itinero.VectorTiles.Tiles;
using Microsoft.AspNetCore.Mvc;

namespace Anyways.VectorTiles.API.Controllers
{
    [ApiController]
    public class TilesController : ControllerBase
    {
        [HttpGet("{tileSet}/mvt.json")]
        public IActionResult Get(string tileSet)
        {
            var tileFileInfo = new FileInfo(Path.Combine(tileSet, $"mvt.json"));

            if (tileFileInfo.Exists)
            {
                return PhysicalFile(tileFileInfo.FullName, "application/json");
            }

            return NotFound();
        }

        [HttpGet("{tileSet}/{z}/{x}/{y}.mvt")]
        public IActionResult Get(string tileSet, int z, int x, int y)
        {
            var t = new Tile(x, y, z);
            var tileFileInfo = new FileInfo(Path.Combine(tileSet, t.Zoom.ToString(), t.X.ToString(), $"{t.Y}.mvt"));

            if (tileFileInfo.Exists)
            {
                return PhysicalFile(tileFileInfo.FullName, "application/x-protobuf");
            }

            return NotFound();
        }
    }
}
