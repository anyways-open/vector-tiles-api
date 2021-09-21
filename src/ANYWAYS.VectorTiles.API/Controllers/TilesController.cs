using System.IO;
using System.Threading.Tasks;
using ANYWAYS.VectorTiles.API.MBTiles;
using Microsoft.AspNetCore.Http;
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
            // check for a mbtiles file first.
            VectorTileSource? mvt = null;
            if (MBTilesReader.TryOpenConnection(Path.Combine(_configuration.DataPath, $"{tileSet}.mbtiles"), out var mbTilesConnection) &&
                mbTilesConnection != null)
            {
                using (mbTilesConnection)
                {
                    // there is an mbtiles file with the given name, try to access it.
                    var metadata = mbTilesConnection.ReadMetaData();
                    mvt = metadata.ToVectorTileSource(tileSet);
                }
            }
            else
            {
                // there is probably a file on disk.
                var tileFileInfo = new FileInfo(Path.Combine(_configuration.DataPath, tileSet, "mvt.json"));
                if (!tileFileInfo.Exists) return NotFound();

                mvt = JsonConvert.DeserializeObject<VectorTileSource>(
                    System.IO.File.ReadAllText(tileFileInfo.FullName));
            }

            // set the tile url.
            var url = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/{tileSet}/{{z}}/{{x}}/{{y}}.mvt";
            mvt.tiles = new[]
            {
                url
            };
                
            return new JsonResult(mvt);
        }

        [HttpGet("{tileSet}/{z}/{x}/{y}.mvt")]
        public async Task<IActionResult> Get(string tileSet, int z, int x, int y)
        {
            // check for a mbtiles file first.
            if (MBTilesReader.TryOpenConnection(Path.Combine(_configuration.DataPath, $"{tileSet}.mbtiles"), out var mbTilesConnection) &&
                mbTilesConnection != null)
            {
                using (mbTilesConnection)
                {
                    // there is an mbtiles file with the given name, try to access it.
                    if (!mbTilesConnection.TryReadTile(z, x, y, out var stream)) return NotFound();
                    
                    HttpContext.Response.Headers.Add("Content-Encoding", new string[] { "gzip" });
                    return File(stream, "application/x-protobuf");
                }
            }
            
            // check for physical files second.
            var tileFileInfo = new FileInfo(Path.Combine(_configuration.DataPath, tileSet, z.ToString(), x.ToString(), $"{y}.mvt"));
            if (tileFileInfo.Exists)
            {
                return PhysicalFile(tileFileInfo.FullName, "application/x-protobuf");
            }

            return NotFound();
        }
    }
}
