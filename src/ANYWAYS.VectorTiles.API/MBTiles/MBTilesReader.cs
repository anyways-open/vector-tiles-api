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

        public static bool TryReadTile(this MBTilesConnection connection, int z, int x, int y, out Stream stream)
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
            if (!reader.Read())
            {
                stream = Stream.Null;
                return false;
            } 
            stream = reader.GetStream(0);
            return true;
        }

        public static Metadata ReadMetaData(this MBTilesConnection connection)
        {
            var command = connection.Connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText =
                $"select name, value from metadata";
            var reader = command.ExecuteReader();
            var metadata = new Metadata();
            while (reader.Read())
            {
                var name = reader.GetString(0);
                switch (name)
                {
                    case "name":
                        metadata.name = reader.GetString(1);
                        break;
                    case "description":
                        metadata.description = reader.GetString(1);
                        break;
                    case "bounds":
                        metadata.bounds = reader.GetString(1);
                        break;
                    case "center":
                        metadata.center = reader.GetString(1);
                        break;
                    case "minzoom":
                        metadata.minzoom = reader.GetString(1);
                        break;
                    case "maxzoom":
                        metadata.maxzoom = reader.GetString(1);
                        break;
                    case "json":
                        metadata.json = reader.GetString(1);
                        break;
                    case "version":
                        metadata.version = reader.GetString(1);
                        break;
                    case "type":
                        metadata.type = reader.GetString(1);
                        break;
                    case "format":
                        metadata.format = reader.GetString(1);
                        break;
                    default:
                        Console.WriteLine(name);
                        break;
                }
            }

            return metadata;
        }
    }
}