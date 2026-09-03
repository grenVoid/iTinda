using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS
{
    internal class Reset
    {
        static ConnectionDB _db = new ConnectionDB();

        public static int GetUserId(string username)
        {
            try
            {
                using (var conn = _db.GetConnection())
                {
                    conn.Open();

                    var cmd = conn.CreateCommand();

                    cmd.CommandText = @"
                SELECT Id
                FROM Admin
                WHERE Username = @username";

                    cmd.Parameters.AddWithValue("@username", username);

                    object? result = cmd.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                        return 0;

                    return Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to get user information.\n\n" +
                    ex.Message,
                    "Reset Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return 0;
            }
        }
        public static bool IsOriginalSetupAccount(int userId)
        {
            try
            {
                using (var conn = _db.GetConnection())
                {
                    conn.Open();

                    var cmd = conn.CreateCommand();

                    cmd.CommandText = @"
                        SELECT MIN(Id)
                        FROM Admin";

                    object? result = cmd.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                        return false;

                    int originalSetupId =
                        Convert.ToInt32(result);

                    return userId == originalSetupId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to verify reset permission.\n\n" +
                    ex.Message,
                    "Reset Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

            }
            return false;
        }
        public static bool ResetApplicationData()
        {
            try
            {
                using (var conn = _db.GetConnection())
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            var cmd = conn.CreateCommand();
                            cmd.Transaction = transaction;

                            cmd.CommandText = @"
                        SELECT MIN(Id)
                        FROM Admin";

                            object? result = cmd.ExecuteScalar();

                            if (result == null || result == DBNull.Value)
                            {
                                transaction.Rollback();
                                return false;
                            }

                            int originalSetupId = Convert.ToInt32(result);

                            cmd.CommandText = "DELETE FROM Sales";
                            cmd.ExecuteNonQuery();

                            cmd.CommandText = "DELETE FROM DailyReports";
                            cmd.ExecuteNonQuery();

                            cmd.CommandText = "DELETE FROM Inventory";
                            cmd.ExecuteNonQuery();

                            cmd.CommandText = @"
                        DELETE FROM Admin
                        WHERE Id <> @originalSetupId";

                            cmd.Parameters.AddWithValue(
                                "@originalSetupId",
                                originalSetupId
                            );

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
                    "Unable to reset application data.\n\n" +
                    ex.Message,
                    "Reset Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return false;
            }
        }
    }
}
