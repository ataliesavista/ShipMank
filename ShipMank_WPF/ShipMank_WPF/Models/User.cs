using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Data;
using ShipMank_WPF.Models;

namespace ShipMank_WPF.Models
{
    public class User
    {
        // ----------------------------------------------------
        // 1. Properti Data (Sesuai Skema PostgreSQL)
        // ----------------------------------------------------
        public int UserID { get; internal set; }
        public string Username { get; set; }
        private string PasswordHash { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string NoTelp { get; set; }
        public string Alamat { get; set; }
        public string Gender { get; set; }
        public DateTime? TTL { get; set; }
        public bool IsGoogleLogin { get; set; } = false;

        public List<Booking> Bookings { get; private set; }
        public List<Review> Reviews { get; private set; }

        private static string ConnectionString => DBHelper.GetConnectionString();

        // Implementasi HashPassword dan VerifyPassword (dibiarkan placeholder, tapi harus diimplementasikan dengan benar)
        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        private static bool VerifyPassword(string passwordRaw, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(passwordRaw, storedHash);
        }

        // ----------------------------------------------------
        // 2. Metode Database Access (CRUD)
        // ----------------------------------------------------

        /// <summary>
        /// Mendaftarkan User baru ke database. Name sekarang dibuat opsional (null).
        /// </summary>
        // MODIFIKASI: Hapus 'name' dari parameter wajib. Name dan lainnya diberi nilai default null.
        public bool Register(string username, string passwordRaw, string email,
                             string name = null, string noTelp = null, string alamat = null,
                             string gender = null, DateTime? ttl = null)
        {
            string PasswordHash = HashPassword(passwordRaw);
            string sql = @"
                INSERT INTO ""User"" 
                (username, password, email, name, noTelp, alamat, gender, ttl) 
                VALUES 
                (@username, @password, @email, @name, @noTelp, @alamat, @gender, @ttl)";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        // Parameter wajib: Username, Password, Email
                        cmd.Parameters.AddWithValue("username", username);
                        cmd.Parameters.AddWithValue("password", PasswordHash);
                        cmd.Parameters.AddWithValue("email", email);

                        // Parameter opsional (gunakan DBNull.Value jika null)
                        cmd.Parameters.AddWithValue("name", (object)name ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("noTelp", (object)noTelp ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("alamat", (object)alamat ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("gender", (object)gender ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("ttl", (object)ttl ?? DBNull.Value);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (NpgsqlException ex)
                {
                    // Catch error, misal: duplicate key (username/email sudah ada)
                    System.Diagnostics.Debug.WriteLine($"DB Error: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Mencari dan memuat data user dari database saat login.
        /// </summary>
        public static User Login(string username, string passwordRaw)
        {
            string sql = "SELECT * FROM \"User\" WHERE username = @username";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                // HANYA SEKALI: conn.Open() dan menggunakan NpgsqlCommand
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(sql, conn)) // Baris ini TIDAK diulang
                    {
                        cmd.Parameters.AddWithValue("username", username);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedHash = reader["password"].ToString();
                                if (VerifyPassword(passwordRaw, storedHash))
                                {
                                    return CreateUserFromReader(reader, storedHash);
                                }
                            }
                        }
                    } // Penutup using cmd
                }
                catch (NpgsqlException ex)
                {
                    // Tangani error koneksi/DB jika perlu
                    System.Diagnostics.Debug.WriteLine($"Login DB Error: {ex.Message}");
                }
            }
            return null;
        }

        public static User GetUserByEmail(string email)
        {
            string sql = "SELECT * FROM \"User\" WHERE email = @email";

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("email", email);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedHash = reader["password"].ToString();
                            return CreateUserFromReader(reader, storedHash);
                        }
                    }
                }
            }
            return null;
        }

        private static User CreateUserFromReader(NpgsqlDataReader reader, string storedHash)
        {
            return new User
            {
                UserID = reader.GetInt32(reader.GetOrdinal("userID")),
                Username = reader.GetString(reader.GetOrdinal("username")),
                PasswordHash = storedHash,
                Email = reader.GetString(reader.GetOrdinal("email")),
                Name = reader.IsDBNull(reader.GetOrdinal("name")) ? null : reader.GetString(reader.GetOrdinal("name")),
                NoTelp = reader.IsDBNull(reader.GetOrdinal("noTelp")) ? null : reader.GetString(reader.GetOrdinal("noTelp")),
                Alamat = reader.IsDBNull(reader.GetOrdinal("alamat")) ? null : reader.GetString(reader.GetOrdinal("alamat")),
                Gender = reader.IsDBNull(reader.GetOrdinal("gender")) ? null : reader.GetString(reader.GetOrdinal("gender")),
                TTL = reader.IsDBNull(reader.GetOrdinal("ttl")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("ttl"))
            };
        }
    }
}