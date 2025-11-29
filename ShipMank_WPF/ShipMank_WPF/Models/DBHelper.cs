using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ShipMank_WPF.Models
{
    public static class DBHelper
    {
        private static IConfigurationRoot _configuration;

        public static string GetConnectionString(string name = "PostgresConnection")
        {
            try
            {
                if (_configuration == null)
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                    var builder = new ConfigurationBuilder()
                        .SetBasePath(baseDirectory)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                    _configuration = builder.Build();
                }

                string connectionString = _configuration.GetConnectionString(name);

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception($"Connection string '{name}' tidak ditemukan di appsettings.json");
                }

                return connectionString;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DBHelper Error] Gagal memuat config: {ex.Message}");
                throw;
            }
        }
    }
}