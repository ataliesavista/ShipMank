using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using ShipMank_WPF.Components;
using ShipMank_WPF.Models;  
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Windows;

namespace ShipMank_WPF.Models.Services
{
    public class HistoryService
    {
        public static void CheckAndProcessCompletions()
        {
            try
            {
                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();
                    string sql = @"UPDATE Booking SET status = 'Completed' 
                                   WHERE status = 'Upcoming' AND dateBerangkat < CURRENT_DATE";
                    using (var cmd = new NpgsqlCommand(sql, conn)) cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }

        public static List<OrderHistoryItem> GetHistoryByUser(int userId)
        {
            var list = new List<OrderHistoryItem>();
            try
            {
                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();
                    string sql = @"
                        SELECT b.bookingID, b.kapalID, k.namaKapal, s.typeName, b.dateBerangkat,
                               l.city, l.province, COALESCE(p.jumlah, k.hargaPerjalanan) AS totalHarga,
                               b.status, p.datePayment, p.paymentMethod,
                               CASE WHEN r.reviewID IS NOT NULL THEN TRUE ELSE FALSE END AS isRated
                        FROM Booking b
                        INNER JOIN Kapal k ON b.kapalID = k.kapalID
                        INNER JOIN ShipType s ON k.shipType = s.typeID
                        LEFT JOIN Lokasi l ON k.lokasi = l.portID
                        LEFT JOIN Payment p ON b.bookingID = p.bookingID
                        LEFT JOIN Reviews r ON b.bookingID = r.bookingID 
                        WHERE b.userID = @UserID
                        ORDER BY b.dateBooking DESC;";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("UserID", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new OrderHistoryItem
                                {
                                    OrderID = $"BKG-{reader["bookingID"].ToString().PadLeft(5, '0')}",
                                    OriginalBookingID = (int)reader["bookingID"],
                                    KapalID = (int)reader["kapalID"],
                                    ItemName = reader["namaKapal"].ToString(),
                                    ShipType = reader["typeName"].ToString(),
                                    Date = Convert.ToDateTime(reader["dateBerangkat"]),
                                    Route = $"{reader["city"] ?? "-"}, {reader["province"] ?? "-"}",
                                    TotalString = $"Rp {Convert.ToDecimal(reader["totalHarga"]):N0}",
                                    TotalAmount = Convert.ToDecimal(reader["totalHarga"]),
                                    Status = reader["status"].ToString(),
                                    PaymentDate = reader["datePayment"] as DateTime?,
                                    PaymentMethod = reader["paymentMethod"]?.ToString() ?? "-",
                                    IsRated = (bool)reader["isRated"]
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading history: " + ex.Message); }
            return list;
        }

        public static async Task SyncUnpaidBookingsAsync()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var config = builder.Build();
            string serverKey = config["Midtrans:ServerKey"];
            bool isProd = bool.Parse(config["Midtrans:IsProduction"]);
            string baseUrl = isProd ? "https://api.midtrans.com/v2" : "https://api.sandbox.midtrans.com/v2";

            var unpaidIds = new List<int>();

            try
            {
                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT bookingID FROM Booking WHERE status = 'Unpaid'", conn))
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read()) unpaidIds.Add((int)reader["bookingID"]);
                }
            }
            catch { return; }

            using (var client = new HttpClient())
            {
                var authString = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(serverKey + ":"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                foreach (var id in unpaidIds)
                {
                    try
                    {
                        var response = await client.GetAsync($"{baseUrl}/BKG-{id}/status");
                        if (!response.IsSuccessStatusCode) continue;

                        var json = await response.Content.ReadAsStringAsync();
                        dynamic data = JsonConvert.DeserializeObject(json);
                        string status = data.transaction_status;

                        if (status == "settlement" || status == "capture")
                        {
                            UpdateToPaid(id);
                        }
                    }
                    catch { }
                }
            }
        }

        private static void UpdateToPaid(int bookingId)
        {
            using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        new NpgsqlCommand("UPDATE Booking SET status = 'Upcoming' WHERE bookingID = " + bookingId, conn, trans).ExecuteNonQuery();
                        new NpgsqlCommand("UPDATE Payment SET paymentStatus = 'Completed' WHERE bookingID = " + bookingId, conn, trans).ExecuteNonQuery();
                        trans.Commit();
                    }
                    catch { trans.Rollback(); }
                }
            }
        }

        // Rating Logic
        public static int GetExistingRating(int bookingId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();
                    var res = new NpgsqlCommand("SELECT ratingValue FROM Reviews WHERE bookingID = " + bookingId, conn).ExecuteScalar();
                    if (res != null && res != DBNull.Value) return Convert.ToInt32(res);
                }
            }
            catch { }
            return 0;
        }

        public static void SubmitRating(int bookingId, int kapalId, int ratingValue)
        {
            using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Insert Review
                        var cmd1 = new NpgsqlCommand("INSERT INTO Reviews (bookingID, ratingValue) VALUES (@bid, @val)", conn, trans);
                        cmd1.Parameters.AddWithValue("bid", bookingId);
                        cmd1.Parameters.AddWithValue("val", ratingValue);
                        cmd1.ExecuteNonQuery();

                        // Recalculate Average
                        var cmd2 = new NpgsqlCommand("SELECT AVG(r.ratingValue) FROM Reviews r JOIN Booking b ON r.bookingID = b.bookingID WHERE b.kapalID = @kid", conn, trans);
                        cmd2.Parameters.AddWithValue("kid", kapalId);
                        double newAvg = Convert.ToDouble(cmd2.ExecuteScalar() ?? 0);

                        // Update Kapal
                        var cmd3 = new NpgsqlCommand("UPDATE Kapal SET rating = @newRate WHERE kapalID = @kid", conn, trans);
                        cmd3.Parameters.AddWithValue("newRate", newAvg);
                        cmd3.Parameters.AddWithValue("kid", kapalId);
                        cmd3.ExecuteNonQuery();

                        trans.Commit();
                        MessageBox.Show("Thank you! Your rating has been submitted.", "Success");
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw ex;
                    }
                }
            }
        }
    }
}