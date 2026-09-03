using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS
{
    internal class Sales
    {
        private static ConnectionDB _db = new ConnectionDB();
        public static DataTable GetProducts()
        {

            DataTable dt = new DataTable();

            using (var conn = _db.GetConnection())
            {
                conn.Open();

                var cmd = conn.CreateCommand();

                cmd.CommandText = @"
            SELECT Id, Product_Name, Product_Details, Stock, Price
            FROM Inventory";

                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }

            return dt;
        }

        public static DataTable SearchProducts(string searchText)
        {

            DataTable dt = new DataTable();

            using (var conn = _db.GetConnection())
            {
                conn.Open();

                var cmd = conn.CreateCommand();

                cmd.CommandText = @"
            SELECT Id, Product_Name, Product_Details, Stock, Price
            FROM Inventory
            WHERE Product_Name LIKE @search";

                cmd.Parameters.AddWithValue(
                    "@search",
                    "%" + searchText + "%"
                );

                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }

            return dt;
        }

        public static void CreateTable()
        {
            try
            {

                using (var conn = _db.GetConnection())
                {
                    conn.Open();

                    var cmd = conn.CreateCommand();

                    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Sales(Id INTEGER PRIMARY KEY AUTOINCREMENT, TotalAmount REAL NOT NULL, DateTime TEXT NOT NULL)";

                    cmd.ExecuteNonQuery();
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                           $"Error creating Sales table: {ex.Message}",
                           "Database Error",
                           MessageBoxButtons.OK,
                           MessageBoxIcon.Error);
            }
        }

        public static bool AddSale(double totalAmount)
        {
            try
            {
                using (var conn = _db.GetConnection())
                {
                    conn.Open();

                    var cmd = conn.CreateCommand();

                    cmd.CommandText = @"
                INSERT INTO Sales (TotalAmount, DateTime)
                VALUES (@totalAmount, @dateTime)";

                    cmd.Parameters.AddWithValue(
                        "@totalAmount",
                        totalAmount
                    );

                    cmd.Parameters.AddWithValue(
                        "@dateTime",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    );

                    int rowsAffected = cmd.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving sale:\n\n{ex.Message}",
                    "Sales Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return false;
            }
        }
    }
}
    
