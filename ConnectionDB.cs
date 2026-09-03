using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS
{
    internal class ConnectionDB
    {
        private static string connection = "Data Source=pos.db";
        public SqliteConnection GetConnection()
        {
            try
            {
                var conn = new SqliteConnection(connection);
                    conn.Open();
                    return conn;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Occur: {ex.Message}","Info",MessageBoxButtons.OK,MessageBoxIcon.Error);
                throw;
            }
        }
    }
}