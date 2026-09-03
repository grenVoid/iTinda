using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POS
{
    internal class Inventory
    {
       private static ConnectionDB _db = new ConnectionDB();

        public static void CreateTable()
        {
            try
            {
                using (var crt = _db.GetConnection())
                {
                    crt.Open();
                    var cmd = crt.CreateCommand();
                    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Inventory(Id INTEGER PRIMARY KEY AUTOINCREMENT, Category TEXT NOT NULL, Product_Name TEXT NOT NULL, Product_Details TEXT NOT NULL, Stock INTEGER NOT NULL, Price REAL NOT NULL, DateTIME DATETIME DEFAULT (datetime('now')))";
                    cmd.ExecuteNonQuery();
                }
            }catch (Exception ex)
            {
                MessageBox.Show($"Error Occur: {ex.Message}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        public static void AddNewItem(string category, string productName, string productDetails, int stock, double price)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO Inventory (Category, Product_Name, Product_Details, Stock, Price) 
                            VALUES (@category, @productName, @productDetails, @stock, @price)";
                cmd.Parameters.AddWithValue("@category", category);
                cmd.Parameters.AddWithValue("@productName", productName);
                cmd.Parameters.AddWithValue("@productDetails", productDetails);
                cmd.Parameters.AddWithValue("@stock", stock);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateInventory(int id, string category, string productName, string productDetails, int stock, double price)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                cmd.CommandText = @"
            UPDATE Inventory 
            SET 
                Category = @category,
                Product_Name = @productName,
                Product_Details = @productDetails,
                Stock = @stock,
                Price = @price,
                DateTIME = @dateTime
            WHERE Id = @id";

                cmd.Parameters.AddWithValue("@category", category);
                cmd.Parameters.AddWithValue("@productName", productName);
                cmd.Parameters.AddWithValue("@productDetails", productDetails);
                cmd.Parameters.AddWithValue("@stock", stock);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.Parameters.AddWithValue(
                    "@dateTime",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                );
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteInventory(int id)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"DELETE FROM Inventory WHERE Id=@id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeductStock(int id, int quantity)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();

                var cmd = conn.CreateCommand();

                cmd.CommandText = @"
            UPDATE Inventory
            SET Stock = Stock - @quantity
            WHERE Id = @id
              AND Stock >= @quantity";

                cmd.Parameters.AddWithValue("@quantity", quantity);
                cmd.Parameters.AddWithValue("@id", id);

                int affectedRows = cmd.ExecuteNonQuery();

                if (affectedRows == 0)
                {
                    throw new Exception(
                        "Insufficient stock or product not found."
                    );
                }
            }
        }

        public static DataTable SearchInventory(string searchText)
        {
            DataTable dt = new DataTable();

            using (var conn = _db.GetConnection())
            {
                conn.Open();

                var cmd = conn.CreateCommand();

                cmd.CommandText = @"
            SELECT * FROM Inventory
            WHERE Category LIKE @search
               OR Product_Name LIKE @search";

                cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");

                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }

            return dt;
        }

    }
}
