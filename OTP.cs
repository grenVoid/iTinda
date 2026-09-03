using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS
{
    internal class OTP
    {
       static ConnectionDB _db = new ConnectionDB();

        private static string generatedOTP = "";
        private static DateTime expirationTime;

        public static bool IsVerified { get; private set; }

        public static string Generate()
        {
            Random random = new Random();

            generatedOTP = random.Next(100000, 1000000).ToString();

            expirationTime = DateTime.Now.AddMinutes(1);

            IsVerified = false;

            return generatedOTP;
        }

        public static bool Verify(string enteredOTP)
        {
            if (string.IsNullOrWhiteSpace(generatedOTP))
                return false;

            if (DateTime.Now >= expirationTime)
            {
                Clear();
                return false;
            }

            if (enteredOTP.Trim() == generatedOTP)
            {
                IsVerified = true;
                return true;
            }

            return false;
        }

        public static bool IsExpired()
        {
            if (string.IsNullOrEmpty(generatedOTP))
                return true;

            return DateTime.Now >= expirationTime;
        }

        public static int GetRemainingSeconds()
        {
            if (string.IsNullOrEmpty(generatedOTP))
                return 0;

            TimeSpan remaining = expirationTime - DateTime.Now;

            if (remaining.TotalSeconds <= 0)
                return 0;

            return (int)Math.Ceiling(remaining.TotalSeconds);
        }

        public static void Clear()
        {
            generatedOTP = "";
            IsVerified = false;
            expirationTime = DateTime.MinValue;
        }

        public static bool ConfirmContact(string readContact)
        {
            try
            {
                using (var get = _db.GetConnection())
                {
                    get.Open();
                    var cmd = get.CreateCommand();
                    cmd.CommandText = "SELECT Count(*) FROM Admin WHERE [Contact] = @contact"; 
                    cmd.Parameters.AddWithValue("@contact", readContact);
                    long? count = (long?)cmd.ExecuteScalar();

                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool UpdatePassword(string contact, string newPassword)
        {
            string newSalt = Setup.GenerateSalt();
            string newHash = Setup.HashPassword(newPassword, newSalt);

            try
            {
                using (var crt = _db.GetConnection())
                {
                    crt.Open();
                    var cmd = crt.CreateCommand();
                    cmd.CommandText = @"UPDATE Admin 
                                SET PasswordHash = @hash, PasswordSalt = @salt 
                                WHERE Contact = @contact";
                    cmd.Parameters.AddWithValue("@hash", newHash);
                    cmd.Parameters.AddWithValue("@salt", newSalt);
                    cmd.Parameters.AddWithValue("@contact", contact);

                    int rowsAffected = cmd.ExecuteNonQuery(); 
                    return rowsAffected > 0;  
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Occur: {ex.Message}", "Info",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
