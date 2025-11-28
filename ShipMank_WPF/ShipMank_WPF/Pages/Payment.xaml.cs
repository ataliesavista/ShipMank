using System;
using System.Windows; // Wajib untuk Application.Current
using System.Windows.Controls;
using Npgsql;
using ShipMank_WPF.Components;
using ShipMank_WPF.Models;
using ShipMank_WPF.Models.Services;
using ShipMank_WPF.Models.ViewModel;

namespace ShipMank_WPF.Pages
{
    public partial class Payment : Page
    {
        private ShipViewModel _shipData;
        private DateTime _bookingDate;

        // HAPUS "= 1", kita set default 0 dulu
        private int _currentUserID = 0;

        private MidtransServices _midtransService;

        public Payment(ShipViewModel shipData, DateTime bookingDate)
        {
            InitializeComponent();
            _shipData = shipData;
            _bookingDate = bookingDate;

            _midtransService = new MidtransServices();

            // =======================================================
            // PERBAIKAN: AMBIL USER ID DARI MAIN WINDOW
            // =======================================================
            if (Application.Current.MainWindow is MainWindow mw && mw.CurrentUser != null)
            {
                _currentUserID = mw.CurrentUser.UserID; // Ambil ID yang Login
            }
            else
            {
                // Fallback jika testing tanpa login / error session
                // MessageBox.Show("Session User tidak ditemukan/Logout. Menggunakan ID default.");
                _currentUserID = 1;
            }
            // =======================================================

            LoadUserData();
            UpdateUI();
        }

        private void LoadUserData()
        {
            // Pengecekan keamanan
            if (_currentUserID == 0) return;

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
                MessageBox.Show("Gagal memuat data user: " + ex.Message);
            }
        }

        private void UpdateUI()
        {
            if (_shipData == null) return;
            TxtNamaKapal.Text = _shipData.ShipName;
            TxtShipType.Text = _shipData.ShipClass;
            TxtDateBerangkat.Text = _bookingDate.ToString("dddd, dd MMM yyyy");
            TxtLokasi.Text = _shipData.FullLocation;
            TxtKapasitas.Text = _shipData.Capacity;

            decimal total = ParseCurrency(_shipData.Price) + 5000;
            TxtHargaPerjalanan.Text = _shipData.Price;
            TxtTotal.Text = $"Rp {total:N0}";
        }

        private async void PayButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (_shipData == null) return;

            // Validasi User ID
            if (_currentUserID == 0)
            {
                MessageBox.Show("Sesi login tidak valid. Silakan login ulang.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PayButton.IsEnabled = false;
            PayButton.Content = "Processing...";

            string bank = RbMandiri.IsChecked == true ? "mandiri" : (RbBni.IsChecked == true ? "bni" : (RbBri.IsChecked == true ? "bri" : "bca"));
            string type = bank == "mandiri" ? "echannel" : "bank_transfer";
            decimal amount = ParseCurrency(TxtTotal.Text);

            var result = await TransactionServices.ProcessBookingTransaction(
                _currentUserID, // <--- Ini sekarang sudah dinamis sesuai yang login
                _shipData.KapalID,
                _bookingDate,
                amount,
                bank,
                type,
                _midtransService);

            if (result.Success)
            {
                var popup = new PaymentResult(bank, result.VaNumber, $"Rp {amount:N0}");
                popup.ShowDialog();

                if (NavigationService.CanGoBack) NavigationService.GoBack();
            }
            else
            {
                MessageBox.Show($"Transaksi Gagal: {result.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                PayButton.IsEnabled = true;
                PayButton.Content = "Confirm & Pay";
            }
        }

        private decimal ParseCurrency(string str) => decimal.TryParse(str?.Replace("Rp", "").Replace(".", "").Trim(), out decimal res) ? res : 0;
        private void CancelButton_Click(object sender, RoutedEventArgs e) { if (NavigationService.CanGoBack) NavigationService.GoBack(); }
    }
}