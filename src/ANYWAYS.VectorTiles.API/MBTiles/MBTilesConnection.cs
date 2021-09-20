using System;
using System.Data.SQLite;
using System.IO;

namespace ANYWAYS.VectorTiles.API.MBTiles
{
    internal class MBTilesConnection : IDisposable
    {
        public SQLiteConnection Connection { get; set; }

        public FileInfo FileInfo { get; set; }

        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}