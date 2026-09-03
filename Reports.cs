using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace POS
{
    internal class Reports
    {
        static ConnectionDB _db = new ConnectionDB();

        public static void GenerateReport()
        {

            try
            {
                using (var crt = _db.GetConnection())
                {
                    crt.Open();

                    var cmd = crt.CreateCommand();
                    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS DailyReports(
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL,
                    Date TEXT NOT NULL,
                    TimeIn TEXT NOT NULL,
                    TimeOut TEXT,
                    ExitReason TEXT, 
                    Receipts INTEGER NOT NULL DEFAULT 0,
                    TotalSales REAL NOT NULL DEFAULT 0)";

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to create DailyReports table.\n\n{ex.Message}",
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        public static void SaveTimeIn(string username)
        {
            try
            {
                using (var crt = _db.GetConnection())
                {
                    crt.Open();

                    string date = DateTime.Now.ToString("yyyy-MM-dd");
                    string timeIn = DateTime.Now.ToString("hh:mm tt");

                    var cmd = crt.CreateCommand();

                    cmd.CommandText = @"
                INSERT INTO DailyReports
                (
                    Username,
                    Date,
                    TimeIn,
                    TimeOut,
                    ExitReason,
                    Receipts,
                    TotalSales
                )
                VALUES
                (
                    @Username,
                    @Date,
                    @TimeIn,
                    NULL,
                    NULL,
                    0,
                    0
                )";

                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Date", date);
                    cmd.Parameters.AddWithValue("@TimeIn", timeIn);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save Time In.\n\n{ex.Message}",
                    "Report Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        public static void SaveTimeOut(string username, string exitReason)
        {
            try
            {
                using (var crt = _db.GetConnection())
                {
                    crt.Open();

                    string date = DateTime.Now.ToString("yyyy-MM-dd");
                    string timeOut = DateTime.Now.ToString("hh:mm tt");

                    int receipts = 0;
                    double totalSales = 0;

                    var salesCmd = crt.CreateCommand();

                    salesCmd.CommandText = @"
                              SELECT
                            COUNT(*),
                            COALESCE(SUM(TotalAmount), 0)
                        FROM Sales
                        WHERE DateTime >= @StartDate
                          AND DateTime < @EndDate";

                    salesCmd.Parameters.AddWithValue(
                        "@StartDate",
                        DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss")
                    );

                    salesCmd.Parameters.AddWithValue(
                        "@EndDate",
                        DateTime.Today
                            .AddDays(1)
                            .ToString("yyyy-MM-dd HH:mm:ss")
                    );

                    using (var reader = salesCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            receipts = Convert.ToInt32(reader.GetValue(0));
                            totalSales = Convert.ToDouble(reader.GetValue(1));
                        }
                    }

                    var cmd = crt.CreateCommand();

                    cmd.CommandText = @"
                UPDATE DailyReports
                SET
                    TimeOut = @TimeOut,
                    ExitReason = @ExitReason,
                    Receipts = @Receipts,
                    TotalSales = @TotalSales
                WHERE Id = (
                    SELECT Id
                    FROM DailyReports
                    WHERE Username = @Username
                      AND Date = @Date
                      AND TimeOut IS NULL
                    ORDER BY Id DESC
                    LIMIT 1
                )";

                    cmd.Parameters.AddWithValue("@TimeOut", timeOut);
                    cmd.Parameters.AddWithValue("@ExitReason", exitReason);
                    cmd.Parameters.AddWithValue("@Receipts", receipts);
                    cmd.Parameters.AddWithValue("@TotalSales", totalSales);
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Date", date);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save Time Out.\n\n{ex.Message}",
                    "Report Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        public static DataTable GetDailyReports()
        {
            DataTable dt = new DataTable();

            try
            {
                using (var crt = _db.GetConnection())
                {
                    crt.Open();

                    var cmd = crt.CreateCommand();

                    cmd.CommandText = @"
                SELECT
                    Username,
                    Date,
                    TimeIn,
                    TimeOut,
                    ExitReason,
                    Receipts,
                    TotalSales
                FROM DailyReports
                ORDER BY Date DESC, Id DESC";

                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load daily reports.\n\n{ex.Message}",
                    "Report Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            return dt;
        }

        public static DataTable GetLatestDailyReport(string username)
        {
            DataTable dt = new DataTable();

            try
            {
                using (var conn = _db.GetConnection())
                {
                    conn.Open();

                    var cmd = conn.CreateCommand();

                    cmd.CommandText = @"
                SELECT
                    Username,
                    Date,
                    TimeIn,
                    TimeOut,
                    ExitReason,
                    Receipts,
                    TotalSales
                FROM DailyReports
                WHERE Username = @Username
                ORDER BY Id DESC
                LIMIT 1";

                    cmd.Parameters.AddWithValue("@Username", username);

                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load latest daily report.\n\n{ex.Message}",
                    "Report Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }

            return dt;
        }

        public static bool ClearTransactionData()
        {
            try
            {
                using (var crt = _db.GetConnection())
                {
                    crt.Open();

                    using (var transaction = crt.BeginTransaction())
                    {
                        try
                        {
                            var cmd = crt.CreateCommand();
                            cmd.Transaction = transaction;

                            cmd.CommandText = @"
                        DELETE FROM Sales";

                            cmd.ExecuteNonQuery();

                            cmd.CommandText = @"
                        DELETE FROM DailyReports";

                            cmd.ExecuteNonQuery();

                            cmd.CommandText = @"
                        DELETE FROM sqlite_sequence
                        WHERE name = 'Sales'
                           OR name = 'DailyReports'";

                            cmd.ExecuteNonQuery();

                            transaction.Commit();

                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to clear transaction data.\n\n{ex.Message}",
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return false;
            }
        }
    }
}
