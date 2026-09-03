using System;
using System.IO;
using System.Windows.Forms;

namespace POS
{
    internal class Database_Backup
    {
        static ConnectionDB _db = new ConnectionDB();

        public static bool CreateBackup()
        {
            try
            {
                string dbPath = Path.Combine(
                    AppContext.BaseDirectory,
                    "pos.db"
                );

                if (!File.Exists(dbPath))
                {
                    MessageBox.Show(
                        "Database file not found!",
                        "Backup",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return false;
                }


                string backupFolder = Path.Combine(
                    AppContext.BaseDirectory,
                    ".backup"
                );

                Directory.CreateDirectory(backupFolder);

                DirectoryInfo directoryInfo =
                    new DirectoryInfo(backupFolder);

                directoryInfo.Attributes |= FileAttributes.Hidden;


                DateTime cutoffDate =
                    DateTime.Now.AddDays(-7);

                string[] backupFiles =
                    Directory.GetFiles(
                        backupFolder,
                        "POS_Backup_*.db"
                    );

                foreach (string file in backupFiles)
                {
                    FileInfo fileInfo =
                        new FileInfo(file);

                    if (fileInfo.LastWriteTime < cutoffDate)
                    {
                        try
                        {
                            fileInfo.Delete();
                        }
                        catch
                        {
                           
                        }
                    }
                }


                string backupPath = Path.Combine(
                    backupFolder,
                    $"POS_Backup_{DateTime.Now:yyyy-MM-dd}.db"
                );



                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }

                using (var conn = _db.GetConnection())
                {
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText =
                            "VACUUM INTO @path";

                        cmd.Parameters.AddWithValue(
                            "@path",
                            backupPath
                        );

                        cmd.ExecuteNonQuery();
                    }
                }

                FileInfo backupFile =
                    new FileInfo(backupPath);

                backupFile.Attributes |= FileAttributes.Hidden;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Backup failed:\n\n{ex.Message}",
                    "Backup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return false;
            }
        }
    }
}