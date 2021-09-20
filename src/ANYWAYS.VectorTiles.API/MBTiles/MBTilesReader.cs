using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace ANYWAYS.VectorTiles.API.MBTiles
{
    internal static class MBTilesReader
    {
        public static bool TryOpenConnection(string file, out MBTilesConnection? connection)
        {
            if (!File.Exists(file))
            {
                connection = null;
                return false;
            }
            
            var sqliteConnection = new SQLiteConnection($"Data Source={file}");
            sqliteConnection.Open();

            connection = new MBTilesConnection()
            {
                Connection = sqliteConnection,
                FileInfo = new FileInfo(file)
            };
            return true;
        }

        public static Stream ReadTile(this MBTilesConnection connection, int z, int x, int y)
        {
            var tmsY = Math.Pow(2, z) - 1 - y;
            var command = connection.Connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText =
                $"select tile_data from tiles t where t.zoom_level = :zoom_level and t.tile_column = :tile_column and t.tile_row = :tile_row";
            command.Parameters.AddWithValue("zoom_level", z);
            command.Parameters.AddWithValue("tile_column", x);
            command.Parameters.AddWithValue("tile_row", tmsY);

            var reader = command.ExecuteReader();
            return reader.GetStream(0);
        }
    }
}