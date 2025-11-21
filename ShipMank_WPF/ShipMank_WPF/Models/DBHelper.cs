using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ShipMank_WPF.Models
{
    public static class DBHelper
    {
        // MODIFIKASI: Mengoreksi typo pada default parameter: "PostgresCon**ne**ction"
        // MODIFIKASI: Menambahkan logging/debug output untuk memverifikasi path dan string koneksi.
        public static string GetConnectionString(string name = "PostgresConnection")
        {
            // MODIFIKASI: Menggunakan AppDomain.CurrentDomain.BaseDirectory untuk path yang lebih andal di WPF
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Tambahkan debug untuk melihat path yang sedang digunakan
            System.Diagnostics.Debug.WriteLine($"Configuration Base Path: {baseDirectory}");

            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(baseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                IConfigurationRoot configuration = builder.Build();

                string connectionString = configuration.GetConnectionString(name);

                // Tambahkan debug untuk melihat hasil Connection String
                if (string.IsNullOrEmpty(connectionString))
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: Connection string '{name}' not found in configuration.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Connection String Loaded: {connectionString}");
                }

                return connectionString;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading configuration: {ex.Message}");
                return null;
            }
        }
    }
}