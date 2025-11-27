using System;
using System.Windows;
using System.Windows.Controls;
using Npgsql; // Driver PostgreSQL
using ShipMank_WPF.Models; // Namespace untuk DBHelper
using ShipMank_WPF.Pages;  // Namespace untuk akses Class BeliTiket

namespace ShipMank_WPF.Pages
{
    public partial class Payment : Page
    {
        // Data yang diterima dari DetailKapal
        private BeliTiket.ShipData _shipData;
        private DateTime _bookingDate;

        // Simulasi User ID (Ganti dengan ID dari Session Login aplikasi Anda)
        private int _currentUserID = 1;

        // Constructor utama yang menerima data dari halaman sebelumnya
        public Payment(BeliTiket.ShipData shipData, DateTime bookingDate)
        {
            InitializeComponent();
            _shipData = shipData;
            _bookingDate = bookingDate;

            LoadUserData();
            UpdateUI();
        }

        // Constructor default (Opsional, untuk menghindari error designer XAML)
        public Payment() { InitializeComponent(); }

        // 1. Load Data User (Hanya untuk ditampilkan di UI)
        private void LoadUserData()
        {
            try
            {
                string connString = DBHelper.GetConnectionString();
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    string sql = "SELECT name, email, noTelp FROM Users WHERE userID = @UserID";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("UserID", _currentUserID);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                TxtFullName.Text = reader["name"].ToString();
                                TxtEmail.Text = reader["email"].ToString();
                                TxtPhone.Text = reader["noTelp"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat data user: {ex.Message}");
            }
        }

        // 2. Update Tampilan UI dengan Data Kapal yang dipilih
        private void UpdateUI()
        {
            if (_shipData == null) return;

            // Isi UI Kanan (Ringkasan Order)
            TxtNamaKapal.Text = _shipData.ShipName;
            TxtShipType.Text = _shipData.ShipClass;
            TxtDateBerangkat.Text = _bookingDate.ToString("dddd, dd MMM yyyy");

            // Lokasi Lengkap (Address, City, Province)
            TxtLokasi.Text = _shipData.FullLocation;
            TxtKapasitas.Text = _shipData.Capacity;

            // Perhitungan Biaya
            decimal hargaDasar = ParseCurrency(_shipData.Price);
            decimal serviceFee = 5000;
            decimal totalBayar = hargaDasar + serviceFee;

            TxtHargaPerjalanan.Text = _shipData.Price;
            TxtTotal.Text = $"Rp {totalBayar:N0}";
        }

        // 3. PROSES SIMPAN KE DATABASE (TRANSACTIONAL)
        private void PayButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (_shipData == null) return;

            try
            {
                string connString = DBHelper.GetConnectionString();

                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    // Gunakan Transaction: Jika simpan Payment gagal, simpan Booking juga dibatalkan
                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // ==========================================
                            // LANGKAH A: INSERT KE TABEL BOOKING
                            // ==========================================
                            // Status kita set 'Upcoming' karena asumsinya user sudah klik Bayar
                            string bookingSql = @"
                                INSERT INTO Booking (userID, kapalID, dateBooking, dateBerangkat, status)
                                VALUES (@UserID, @KapalID, NOW(), @DateBerangkat, 'Upcoming')
                                RETURNING bookingID;
                            ";

                            int newBookingID;
                            using (var cmdBooking = new NpgsqlCommand(bookingSql, conn, trans))
                            {
                                cmdBooking.Parameters.AddWithValue("UserID", _currentUserID);
                                cmdBooking.Parameters.AddWithValue("KapalID", _shipData.KapalID);
                                cmdBooking.Parameters.AddWithValue("DateBerangkat", _bookingDate);

                                // ExecuteScalar digunakan untuk mengambil nilai 'RETURNING bookingID'
                                newBookingID = (int)cmdBooking.ExecuteScalar();
                            }

                            // ==========================================
                            // LANGKAH B: INSERT KE TABEL PAYMENT
                            // ==========================================
                            decimal jumlahBayar = ParseCurrency(TxtTotal.Text);

                            // PENTING: Database Anda memiliki constraint CHECK (paymentMethod = 'VirtualAccount')
                            // Jadi kita harus mengirim string 'VirtualAccount' agar tidak error.
                            // Di pengembangan selanjutnya, Anda bisa mengubah constraint DB agar bisa menerima 'Credit Card', 'E-Wallet', dll.
                            string paymentMethod = "VirtualAccount";

                            string paymentSql = @"
                                INSERT INTO Payment (bookingID, paymentMethod, jumlah, paymentStatus, datePayment)
                                VALUES (@BookingID, @PaymentMethod, @Jumlah, 'Completed', NOW());
                            ";

                            using (var cmdPayment = new NpgsqlCommand(paymentSql, conn, trans))
                            {
                                cmdPayment.Parameters.AddWithValue("BookingID", newBookingID);
                                cmdPayment.Parameters.AddWithValue("PaymentMethod", paymentMethod);
                                cmdPayment.Parameters.AddWithValue("Jumlah", jumlahBayar);
                                cmdPayment.ExecuteNonQuery();
                            }

                            // Jika kode sampai sini tanpa error, simpan permanen ke database
                            trans.Commit();

                            MessageBox.Show("Pembayaran Berhasil! Booking telah dikonfirmasi.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Navigasi kembali ke halaman awal atau halaman tiket saya
                            if (NavigationService.CanGoBack)
                            {
                                NavigationService.GoBack();
                            }
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback(); // Batalkan semua perubahan jika ada error
                            throw ex; // Lempar error ke catch luar untuk ditampilkan
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Terjadi kesalahan transaksi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Helper: Mengubah string "Rp 1.000.000" menjadi decimal murni
        private decimal ParseCurrency(string currencyString)
        {
            if (string.IsNullOrEmpty(currencyString)) return 0;
            string clean = currencyString.Replace("Rp", "").Replace(".", "").Replace(" ", "").Trim();
            if (decimal.TryParse(clean, out decimal result)) return result;
            return 0;
        }

        // Tombol Cancel (Back)
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}