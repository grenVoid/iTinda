using System;
using System.Data.SqlTypes;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace POS
{
    internal class Setup
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
                    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Admin(
                                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                        Username TEXT UNIQUE NOT NULL, 
                                        PasswordHash TEXT NOT NULL, 
                                        PasswordSalt TEXT NOT NULL, 
                                        Contact TEXT NOT NULL,
                                        Role TEXT DEFAULT 'Cashier')";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Occur: {ex.Message}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];

            RandomNumberGenerator.Fill(saltBytes);

            return Convert.ToBase64String(saltBytes);
        }

        public static string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256))
            {
                byte[] hashBytes = pbkdf2.GetBytes(32);

                return Convert.ToBase64String(hashBytes);
            }
        }

        public static void CreateAdminUser(string username, string password, string contact)
        {
            string salt = GenerateSalt();
            string passwordHash = HashPassword(password, salt);

            try
            {
                using (var crt = _db.GetConnection())
                {
                    crt.Open();
                    var cmd = crt.CreateCommand();
                    cmd.CommandText = @"INSERT INTO Admin (Username, PasswordHash, PasswordSalt, Contact)
                                        VALUES (@username, @passwordHash, @salt, @contact)";
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@passwordHash", passwordHash);
                    cmd.Parameters.AddWithValue("@salt", salt);
                    cmd.Parameters.AddWithValue("@contact", contact);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Occur: {ex.Message}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static bool Login(string username, string inputPassword)
        {
            try
            {
                using (var crt = _db.GetConnection())
                {
                    crt.Open();
                    var cmd = crt.CreateCommand();
                    cmd.CommandText = "SELECT PasswordHash, PasswordSalt FROM Admin WHERE Username = @user";

                    cmd.Parameters.AddWithValue("@user", username);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var storedHash = reader["PasswordHash"].ToString();
                            var storedSalt = reader["PasswordSalt"].ToString();
                            if(string.IsNullOrEmpty(storedSalt))
                            {
                                return false;
                            }
                            string hashOfInput = HashPassword(inputPassword, storedSalt);

                            if (hashOfInput == storedHash)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Occur: {ex.Message}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return false;
        }
        public static bool AdminExists()
        {
            try
            {
                using (var crt = _db.GetConnection())
                {
                    crt.Open();
                    var cmd = crt.CreateCommand();
                    cmd.CommandText = "SELECT COUNT(*) FROM Admin";

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Occur: {ex.Message}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
        }
    }
}