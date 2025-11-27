using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Npgsql;
using ShipMank_WPF.Models;
using ShipMank_WPF.Components; // Agar bisa panggil RatingWindow

namespace ShipMank_WPF.Pages
{
    public partial class History : Page
    {
        private int _currentUserID = 1; // Sesuaikan dengan ID User Login Anda

        public History()
        {
            InitializeComponent();
            LoadHistoryData();
        }

        private void LoadHistoryData()
        {
            var historyItems = new List<OrderHistoryItem>();

            try
            {
                using (var conn = new NpgsqlConnection(DBHelper.GetConnectionString()))
                {
                    conn.Open();

                    // QUERY BARU: Mengecek tabel Reviews untuk status IsRated
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
                        LEFT JOIN Reviews r ON b.bookingID = r.bookingID -- Join untuk cek rating
                        WHERE b.userID = @UserID
                        ORDER BY b.dateBooking DESC;
                    ";

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
                                    KapalID = (int)reader["kapalID"], // ID Kapal disimpan untuk update rata-rata
                                    ItemName = reader["namaKapal"].ToString(),
                                    ShipType = reader["typeName"].ToString(),
                                    Date = Convert.ToDateTime(reader["dateBerangkat"]),
                                    Route = $"{reader["city"] ?? "-"}, {reader["province"] ?? "-"}",
                                    TotalString = $"Rp {priceVal:N0}",
                                    TotalAmount = priceVal,
                                    Status = reader["status"].ToString(),
                                    PaymentDate = reader["datePayment"] as DateTime?,
                                    PaymentMethod = reader["paymentMethod"]?.ToString() ?? "-",
                                    IsRated = (bool)reader["isRated"] // True jika sudah direview
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
        // EVENT HANDLER TOMBOL RATE
        // ==========================================
        private void BtnRate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is OrderHistoryItem item)
            {
                RatingWindow ratingPopup = new RatingWindow();

                // LOGIKA BARU: Cek apakah user sudah pernah rating?
                if (item.IsRated)
                {
                    // CASE 1: SUDAH RATING (VIEW ONLY)
                    // Ambil nilai rating lama dari database
                    int existingRating = GetUserRatingForBooking(item.OriginalBookingID);

                    // Set window ke mode ReadOnly
                    ratingPopup.SetReadOnlyMode(existingRating);
                    ratingPopup.ShowDialog();
                    // Tidak perlu logic submit karena tombol submit disembunyikan
                }
                else
                {
                    // CASE 2: BELUM RATING (INPUT MODE)
                    bool? result = ratingPopup.ShowDialog();

                    if (result == true)
                    {
                        int userRating = ratingPopup.SelectedRating;
                        SubmitRatingToDatabase(item, userRating);
                        LoadHistoryData(); // Refresh UI agar IsRated terupdate
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
                        if (res != null && res != DBNull.Value)
                        {
                            rating = Convert.ToInt32(res);
                        }
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
                    // Gunakan Transaction agar Insert Review & Update Kapal terjadi bersamaan
                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // A. Insert ke tabel Reviews
                            // Menggunakan bookingID sebagai referensi unik
                            string insertSql = "INSERT INTO Reviews (bookingID, ratingValue) VALUES (@bid, @val)";
                            using (var cmd1 = new NpgsqlCommand(insertSql, conn, trans))
                            {
                                cmd1.Parameters.AddWithValue("bid", item.OriginalBookingID);
                                cmd1.Parameters.AddWithValue("val", ratingValue);
                                cmd1.ExecuteNonQuery();
                            }

                            // B. Hitung Ulang Rata-rata Rating Kapal tersebut
                            // Cari semua review yang booking-nya milik kapalID ini
                            string calcSql = @"
                                SELECT AVG(r.ratingValue) 
                                FROM Reviews r
                                JOIN Booking b ON r.bookingID = b.bookingID
                                WHERE b.kapalID = @kid";

                            double newAverage = 0;
                            using (var cmd2 = new NpgsqlCommand(calcSql, conn, trans))
                            {
                                cmd2.Parameters.AddWithValue("kid", item.KapalID);
                                object result = cmd2.ExecuteScalar();
                                if (result != DBNull.Value)
                                {
                                    newAverage = Convert.ToDouble(result);
                                }
                            }

                            // C. Update Tabel Kapal dengan Rata-rata Baru
                            string updateSql = "UPDATE Kapal SET rating = @newRate WHERE kapalID = @kid";
                            using (var cmd3 = new NpgsqlCommand(updateSql, conn, trans))
                            {
                                cmd3.Parameters.AddWithValue("newRate", newAverage);
                                cmd3.Parameters.AddWithValue("kid", item.KapalID);
                                cmd3.ExecuteNonQuery();
                            }

                            trans.Commit(); // Simpan permanen
                            MessageBox.Show("Thank you! Your rating has been submitted.", "Success");
                        }
                        catch (Exception)
                        {
                            trans.Rollback(); // Batalkan semua jika error
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to submit rating: {ex.Message}");
            }
        }

        private void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is OrderHistoryItem selectedOrder)
            {
                NavigationService.Navigate(new ViewDetails(selectedOrder));
            }
        }

        private void HistoryDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
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
