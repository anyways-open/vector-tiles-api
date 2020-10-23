using System.IO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ANYWAYS.VectorTiles.API.Controllers
{
    [ApiController]
    public class TilesController : ControllerBase
    {
        private readonly StartupConfiguration _configuration;

        public TilesController(StartupConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        [HttpGet("{tileSet}/mvt.json")]
        public IActionResult Get(string tileSet)
        {
            var tileFileInfo = new FileInfo(Path.Combine(_configuration.DataPath, tileSet, "mvt.json"));

            if (!tileFileInfo.Exists) return NotFound();
            
            var mvt = JsonConvert.DeserializeObject<VectorTileSource>(
                System.IO.File.ReadAllText(tileFileInfo.FullName));

            var url = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/{tileSet}/{{z}}/{{x}}/{{y}}.mvt";
            mvt.tiles = new[]
            {
                url
            };
                
            return new JsonResult(mvt);
        }

        [HttpGet("{tileSet}/{z}/{x}/{y}.mvt")]
        public IActionResult Get(string tileSet, int z, int x, int y)
        {
            var tileFileInfo = new FileInfo(Path.Combine(_configuration.DataPath, tileSet, z.ToString(), x.ToString(), $"{y}.mvt"));

            if (tileFileInfo.Exists)
            {
                return PhysicalFile(tileFileInfo.FullName, "application/x-protobuf");
            }

            return NotFound();
        }
    }
}
