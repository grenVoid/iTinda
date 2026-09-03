using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS
{
    internal class Dashboard
    {
        private static ConnectionDB _db = new ConnectionDB();

        public static DataTable GetStockOverview()
        {
            DataTable dt = new DataTable();

            using (var conn = _db.GetConnection())
            {
                conn.Open();

                var cmd = conn.CreateCommand();

                cmd.CommandText = @"
                    SELECT
                        DateTIME,
                        Product_Name,
                        Product_Details,
                        Stock
                    FROM Inventory
                    ORDER BY DateTIME DESC";

                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }

            return dt;
        }

        public static double GetTodaysSales()
        {
            try
            {
                using (var conn = _db.GetConnection())
                {
                    conn.Open();

                    var cmd = conn.CreateCommand();

                    DateTime startOfDay = DateTime.Today;
                    DateTime startOfTomorrow = startOfDay.AddDays(1);

                    cmd.CommandText = @"
                SELECT COALESCE(SUM(TotalAmount), 0)
                FROM Sales
                WHERE DateTime >= @startOfDay
                AND DateTime < @startOfTomorrow";

                    cmd.Parameters.AddWithValue(
                        "@startOfDay",
                        startOfDay.ToString("yyyy-MM-dd HH:mm:ss")
                    );

                    cmd.Parameters.AddWithValue(
                        "@startOfTomorrow",
                        startOfTomorrow.ToString("yyyy-MM-dd HH:mm:ss")
                    );

                    object? result = cmd.ExecuteScalar();   

                    return result == DBNull.Value || result == null
                        ? 0
                        : Convert.ToDouble(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading today's sales:\n\n{ex.Message}",
                    "Sales Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return 0;
            }
        }
    }
}
