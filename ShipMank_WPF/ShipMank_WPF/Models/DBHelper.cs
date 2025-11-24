using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ShipMank_WPF.Models
{
    public static class DBHelper
    {
        // Cache konfigurasi agar tidak membaca file berulang-ulang setiap kali query
        private static IConfigurationRoot _configuration;

        public static string GetConnectionString(string name = "PostgresConnection")
        {
            try
            {
                if (_configuration == null)
                {
                    // Mendapatkan folder tempat .exe aplikasi berjalan (Bin/Debug atau Bin/Release)
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                    var builder = new ConfigurationBuilder()
                        .SetBasePath(baseDirectory)
                        // PENTING: optional: false artinya jika file tidak ada, aplikasi akan error (bagus untuk debugging)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                    _configuration = builder.Build();
                }

                string connectionString = _configuration.GetConnectionString(name);

                // Validasi sederhana
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception($"Connection string '{name}' tidak ditemukan di appsettings.json");
                }

                return connectionString;
            }
            catch (Exception ex)
            {
                // Log error ke Output Window di Visual Studio
                System.Diagnostics.Debug.WriteLine($"[DBHelper Error] Gagal memuat config: {ex.Message}");

                // Melempar error agar Anda sadar ada yang salah saat development
                throw;
            }
        }
    }
}