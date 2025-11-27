using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Npgsql;
using ShipMank_WPF.Models;
using ShipMank_WPF.Components;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using System.IO;
using Newtonsoft.Json;
using System.Text;

namespace ShipMank_WPF.Pages
{
    public partial class History : Page
    {
        private int _currentUserID = 1;

        public History()
        {
            InitializeComponent();
            InitializeHistoryAsync();
        }

        private async void InitializeHistoryAsync()
        {
            CheckAndProcessCompletions(); // 1. Update status Completed jika lewat tanggal
            LoadHistoryData();            // 2. Load data awal
            await SyncPaymentStatusAsync(); // 3. Cek Midtrans (Unpaid -> Upcoming)
            LoadHistoryData();            // 4. Refresh data setelah update
        }

        // ==========================================
        // 1. LOGIKA AUTO-COMPLETE (BY DATE)
        // ==========================================
        private void CheckAndProcessCompletions()
        {
            try
            {
                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();
                    // Query: Ubah status 'Upcoming' menjadi 'Completed' JIKA dateBerangkat < Hari Ini
                    string sql = @"
                        UPDATE Booking 
                        SET status = 'Completed' 
                        WHERE status = 'Upcoming' AND dateBerangkat < CURRENT_DATE";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { /* Ignore error untuk proses background */ }
        }

        // ==========================================
        // 2. LOGIKA SINKRONISASI MIDTRANS
        // ==========================================
        private async Task SyncPaymentStatusAsync()
        {
            // Ambil Config
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var config = builder.Build();
            string serverKey = config["Midtrans:ServerKey"];
            bool isProd = bool.Parse(config["Midtrans:IsProduction"]);
            string baseUrl = isProd ? "https://api.midtrans.com/v2" : "https://api.sandbox.midtrans.com/v2";

            // Ambil List Booking ID yang masih Unpaid
            var unpaidBookings = new List<int>();
            using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
            {
                conn.Open();
                string sql = "SELECT bookingID FROM Booking WHERE status = 'Unpaid'";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) unpaidBookings.Add((int)reader["bookingID"]);
                }
            }

            // Cek API Midtrans satu per satu
            using (var client = new HttpClient())
            {
                var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes(serverKey + ":"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                foreach (var bookingId in unpaidBookings)
                {
                    try
                    {
                        string orderId = $"BKG-{bookingId}";
                        string url = $"{baseUrl}/{orderId}/status";

                        var response = await client.GetAsync(url);
                        if (!response.IsSuccessStatusCode) continue;

                        var jsonString = await response.Content.ReadAsStringAsync();
                        dynamic statusData = JsonConvert.DeserializeObject(jsonString);
                        string transactionStatus = statusData.transaction_status;

                        // Jika Settlement/Capture -> Update jadi Paid
                        if (transactionStatus == "settlement" || transactionStatus == "capture")
                        {
                            UpdateDatabaseToPaid(bookingId);
                        }
                    }
                    catch { /* Ignore error connection */ }
                }
            }
        }

        private void UpdateDatabaseToPaid(int bookingId)
        {
            using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Update Booking -> Upcoming
                        string sql1 = "UPDATE Booking SET status = 'Upcoming' WHERE bookingID = @bid";
                        using (var cmd = new NpgsqlCommand(sql1, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("bid", bookingId);
                            cmd.ExecuteNonQuery();
                        }

                        // Update Payment -> Completed
                        string sql2 = "UPDATE Payment SET paymentStatus = 'Completed' WHERE bookingID = @bid";
                        using (var cmd = new NpgsqlCommand(sql2, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("bid", bookingId);
                            cmd.ExecuteNonQuery();
                        }
                        trans.Commit();
                    }
                    catch { trans.Rollback(); }
                }
            }
        }

        // ==========================================
        // 3. LOGIKA LOAD DATA
        // ==========================================
        private void LoadHistoryData()
        {
            var historyItems = new List<OrderHistoryItem>();
            try
            {
                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();
                    string sql = @"
                        SELECT 
                            b.bookingID, b.kapalID, k.namaKapal, s.typeName, b.dateBerangkat,
                            l.city, l.province,
                            COALESCE(p.jumlah, k.hargaPerjalanan) AS totalHarga,
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
                        cmd.Parameters.AddWithValue("UserID", _currentUserID);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                decimal priceVal = Convert.ToDecimal(reader["totalHarga"]);
                                historyItems.Add(new OrderHistoryItem
                                {
                                    OrderID = $"BKG-{reader["bookingID"].ToString().PadLeft(5, '0')}",
                                    OriginalBookingID = (int)reader["bookingID"],
                                    KapalID = (int)reader["kapalID"],
                                    ItemName = reader["namaKapal"].ToString(),
                                    ShipType = reader["typeName"].ToString(),
                                    Date = Convert.ToDateTime(reader["dateBerangkat"]),
                                    Route = $"{reader["city"] ?? "-"}, {reader["province"] ?? "-"}",
                                    TotalString = $"Rp {priceVal:N0}",
                                    TotalAmount = priceVal,
                                    Status = reader["status"].ToString(),
                                    PaymentDate = reader["datePayment"] as DateTime?,
                                    PaymentMethod = reader["paymentMethod"]?.ToString() ?? "-",
                                    IsRated = (bool)reader["isRated"]
                                });
                            }
                        }
                    }
                }
                HistoryDataGrid.ItemsSource = historyItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading history: {ex.Message}");
            }
        }

        // ==========================================
        // 4. EVENT HANDLERS
        // ==========================================
        private void BtnRate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is OrderHistoryItem item)
            {
                RatingWindow ratingPopup = new RatingWindow();
                if (item.IsRated)
                {
                    int existingRating = GetUserRatingForBooking(item.OriginalBookingID);
                    ratingPopup.SetReadOnlyMode(existingRating);
                    ratingPopup.ShowDialog();
                }
                else
                {
                    bool? result = ratingPopup.ShowDialog();
                    if (result == true)
                    {
                        SubmitRatingToDatabase(item, ratingPopup.SelectedRating);
                        LoadHistoryData(); // Refresh UI
                    }
                }
            }
        }

        private int GetUserRatingForBooking(int bookingId)
        {
            int rating = 0;
            try
            {
                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();
                    string sql = "SELECT ratingValue FROM Reviews WHERE bookingID = @bid";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("bid", bookingId);
                        object res = cmd.ExecuteScalar();
                        if (res != null && res != DBNull.Value) rating = Convert.ToInt32(res);
                    }
                }
            }
            catch { }
            return rating;
        }

        private void SubmitRatingToDatabase(OrderHistoryItem item, int ratingValue)
        {
            try
            {
                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            string insertSql = "INSERT INTO Reviews (bookingID, ratingValue) VALUES (@bid, @val)";
                            using (var cmd1 = new NpgsqlCommand(insertSql, conn, trans))
                            {
                                cmd1.Parameters.AddWithValue("bid", item.OriginalBookingID);
                                cmd1.Parameters.AddWithValue("val", ratingValue);
                                cmd1.ExecuteNonQuery();
                            }

                            string calcSql = "SELECT AVG(r.ratingValue) FROM Reviews r JOIN Booking b ON r.bookingID = b.bookingID WHERE b.kapalID = @kid";
                            double newAverage = 0;
                            using (var cmd2 = new NpgsqlCommand(calcSql, conn, trans))
                            {
                                cmd2.Parameters.AddWithValue("kid", item.KapalID);
                                object result = cmd2.ExecuteScalar();
                                if (result != DBNull.Value) newAverage = Convert.ToDouble(result);
                            }

                            string updateSql = "UPDATE Kapal SET rating = @newRate WHERE kapalID = @kid";
                            using (var cmd3 = new NpgsqlCommand(updateSql, conn, trans))
                            {
                                cmd3.Parameters.AddWithValue("newRate", newAverage);
                                cmd3.Parameters.AddWithValue("kid", item.KapalID);
                                cmd3.ExecuteNonQuery();
                            }
                            trans.Commit();
                            MessageBox.Show("Thank you! Your rating has been submitted.", "Success");
                        }
                        catch { trans.Rollback(); throw; }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"Failed: {ex.Message}"); }
        }

        private void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is OrderHistoryItem selectedOrder)
            {
                NavigationService.Navigate(new ViewDetails(selectedOrder));
            }
        }

        // Method kosong untuk mengatasi Error CS1061 di XAML
        private void HistoryDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Tidak perlu ada logic, biarkan kosong
        }
    }
}

// Model Data untuk DataGrid dan Transfer ke ViewDetails
public class OrderHistoryItem
{
    // ... properti lainnya (OrderID, OriginalBookingID, KapalID, dll) TETAP SAMA ...
    public string OrderID { get; set; }
    public int OriginalBookingID { get; set; }
    public int KapalID { get; set; }
    public string ItemName { get; set; }
    public string ShipType { get; set; }
    public DateTime Date { get; set; }
    public string Route { get; set; }
    public string TotalString { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string PaymentMethod { get; set; }

    public bool IsRated { get; set; }

    // MODIFIKASI DISINI: 
    // Hapus "!IsRated". Jadi tombol Rate selalu muncul kalau Completed.
    public Visibility RateButtonVisibility =>
        (Status == "Completed") ? Visibility.Visible : Visibility.Collapsed;

    // Opsional: Anda bisa mengubah teks tombol di UI nanti (misal jadi "My Rating"),
    // tapi kalau mau tetap "Rate" juga tidak masalah.
}
