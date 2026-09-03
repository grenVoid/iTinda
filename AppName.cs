using System;

namespace POS
{
    internal class AppName
    {
        static ConnectionDB _db = new ConnectionDB();

        public static void CreateTable()
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS AppSettings (
                        Id INTEGER PRIMARY KEY CHECK (Id = 1),
                        AppName TEXT NOT NULL,
                        Address TEXT NOT NULL DEFAULT '',
                        PrintEnabled INTEGER NOT NULL DEFAULT 1
                    );

                    INSERT OR IGNORE INTO AppSettings
                        (Id, AppName, Address, PrintEnabled)
                    VALUES
                        (1, 'iTinda', '', 1);
                ";

                cmd.ExecuteNonQuery();

            }
        }

        public static void UpdateAppName(string appName)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
                    UPDATE AppSettings
                    SET AppName = @AppName
                    WHERE Id = 1;
                ";

                cmd.Parameters.AddWithValue(
                    "@AppName",
                    appName
                );

                cmd.ExecuteNonQuery();
            }
        }


        public static string GetAppName()
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
                    SELECT AppName
                    FROM AppSettings
                    WHERE Id = 1;
                ";

                object? result = cmd.ExecuteScalar();

                return result?.ToString() ?? "iTinda";
            }
        }

        public static void UpdateAddress(string address)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
                    UPDATE AppSettings
                    SET Address = @Address
                    WHERE Id = 1;
                ";

                cmd.Parameters.AddWithValue(
                    "@Address",
                    address
                );

                cmd.ExecuteNonQuery();
            }
        }


        public static string GetAddress()
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
                    SELECT Address
                    FROM AppSettings
                    WHERE Id = 1;
                ";

                object? result = cmd.ExecuteScalar();

                return result?.ToString() ?? "";
            }
        }

        public static string GetOriginalAdminContact()
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
                    SELECT Contact
                    FROM Admin
                    ORDER BY Id ASC
                    LIMIT 1;
                ";

                object? result = cmd.ExecuteScalar();

                return result?.ToString() ?? "";
            }
        }

        public static bool GetPrintEnabled()
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
                    SELECT PrintEnabled
                    FROM AppSettings
                    WHERE Id = 1;
                ";

                object? result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    return true;

                return Convert.ToInt32(result) == 1;
            }
        }


        public static void SetPrintEnabled(bool enabled)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
                    UPDATE AppSettings
                    SET PrintEnabled = @PrintEnabled
                    WHERE Id = 1;
                ";

                cmd.Parameters.AddWithValue(
                    "@PrintEnabled",
                    enabled ? 1 : 0
                );

                cmd.ExecuteNonQuery();
            }
        }
    }
}