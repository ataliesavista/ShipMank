using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ShipMank_WPF.Models;
using ShipMank_WPF.Pages;

namespace ShipMank_WPF.Pages
{
    public partial class Payment : Page
    {
        private BeliTiket.ShipData _shipData;
        private DateTime _bookingDate;
        private int _currentUserID = 1; // Sesuaikan dengan session login

        // Config Midtrans
        private string _midtransServerKey;
        private string _midtransBaseUrl;

        public Payment(BeliTiket.ShipData shipData, DateTime bookingDate)
        {
            InitializeComponent();
            _shipData = shipData;
            _bookingDate = bookingDate;

            LoadConfiguration();
            LoadUserData();
            UpdateUI();
        }

        public Payment() { InitializeComponent(); LoadConfiguration(); }

        private void LoadConfiguration()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                var config = builder.Build();

                _midtransServerKey = config["Midtrans:ServerKey"];
                bool isProd = bool.Parse(config["Midtrans:IsProduction"]);
                _midtransBaseUrl = isProd ? "https://api.midtrans.com/v2" : "https://api.sandbox.midtrans.com/v2";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load config appsettings.json: " + ex.Message);
            }
        }

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
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void UpdateUI()
        {
            if (_shipData == null) return;
            TxtNamaKapal.Text = _shipData.ShipName;
            TxtShipType.Text = _shipData.ShipClass;
            TxtDateBerangkat.Text = _bookingDate.ToString("dddd, dd MMM yyyy");
            TxtLokasi.Text = _shipData.FullLocation;
            TxtKapasitas.Text = _shipData.Capacity;
            decimal hargaDasar = ParseCurrency(_shipData.Price);
            decimal serviceFee = 5000;
            decimal totalBayar = hargaDasar + serviceFee;
            TxtHargaPerjalanan.Text = _shipData.Price;
            TxtTotal.Text = $"Rp {totalBayar:N0}";
        }

        // ==========================================
        // MAIN PAYMENT LOGIC
        // ==========================================
        private async void PayButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (_shipData == null) return;

            PayButton.IsEnabled = false;
            PayButton.Content = "Processing to Midtrans...";

            string connString = DBHelper.GetConnectionString();

            // 1. Tentukan Bank & Tipe API
            string selectedBank = "bca";
            string paymentType = "bank_transfer";

            if (RbBca.IsChecked == true) selectedBank = "bca";
            else if (RbBni.IsChecked == true) selectedBank = "bni";
            else if (RbBri.IsChecked == true) selectedBank = "bri";
            else if (RbMandiri.IsChecked == true)
            {
                selectedBank = "mandiri";
                paymentType = "echannel"; // Mandiri pakai Bill Payment (E-Channel)
            }

            decimal jumlahBayar = ParseCurrency(TxtTotal.Text);

            // ID Order akan diisi setelah Booking Insert
            string uniqueOrderID = "";

            using (var conn = new NpgsqlConnection(connString))
            {
                await conn.OpenAsync();

                // PENTING: Gunakan BeginTransaction biasa (Synchronous) untuk Npgsql
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // A. INSERT BOOKING (Status Awal: Unpaid)
                        string bookingSql = @"
                            INSERT INTO Booking (userID, kapalID, dateBooking, dateBerangkat, status)
                            VALUES (@UserID, @KapalID, NOW(), @DateBerangkat, 'Unpaid')
                            RETURNING bookingID;";

                        int newBookingID;
                        using (var cmdBooking = new NpgsqlCommand(bookingSql, conn, trans))
                        {
                            cmdBooking.Parameters.AddWithValue("UserID", _currentUserID);
                            cmdBooking.Parameters.AddWithValue("KapalID", _shipData.KapalID);
                            cmdBooking.Parameters.AddWithValue("DateBerangkat", _bookingDate);
                            newBookingID = (int)await cmdBooking.ExecuteScalarAsync();
                        }

                        // SET ORDER ID AGAR BISA DILACAK
                        uniqueOrderID = $"BKG-{newBookingID}";

                        // B. REQUEST KE MIDTRANS (Dapatkan Nomor VA dulu)
                        string vaNumber = await CreateMidtransVaAsync(selectedBank, (long)jumlahBayar, uniqueOrderID, paymentType);

                        // C. INSERT PAYMENT (Status: Unpaid, Simpan VA Number)
                        string paymentSql = @"
                            INSERT INTO Payment (bookingID, paymentMethod, jumlah, paymentStatus, datePayment, va_number)
                            VALUES (@BookingID, 'VirtualAccount', @Jumlah, 'Unpaid', NOW(), @VA);";

                        using (var cmdPayment = new NpgsqlCommand(paymentSql, conn, trans))
                        {
                            cmdPayment.Parameters.AddWithValue("BookingID", newBookingID);
                            cmdPayment.Parameters.AddWithValue("Jumlah", jumlahBayar);
                            cmdPayment.Parameters.AddWithValue("VA", vaNumber); // Simpan Nomor VA ke DB
                            await cmdPayment.ExecuteNonQueryAsync();
                        }

                        // D. COMMIT TRANSACTION
                        trans.Commit();

                        // E. TAMPILKAN POPUP SUKSES
                        ShowSuccessPopup(selectedBank, vaNumber, jumlahBayar);
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show($"Transaksi Gagal: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        PayButton.IsEnabled = true;
                        PayButton.Content = "Confirm & Pay";
                    }
                }
            }
        }

        // Fungsi Request ke API Midtrans
        private async Task<string> CreateMidtransVaAsync(string bank, long amount, string orderId, string type)
        {
            using (var client = new HttpClient())
            {
                var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes(_midtransServerKey + ":"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                object requestData;

                if (type == "echannel") // KHUSUS MANDIRI
                {
                    requestData = new
                    {
                        payment_type = "echannel",
                        transaction_details = new { order_id = orderId, gross_amount = amount },
                        echannel = new { bill_info1 = "Payment For:", bill_info2 = "Ship Ticket" }
                    };
                }
                else // BCA, BNI, BRI
                {
                    requestData = new
                    {
                        payment_type = "bank_transfer",
                        transaction_details = new { order_id = orderId, gross_amount = amount },
                        bank_transfer = new { bank = bank }
                    };
                }

                var jsonContent = JsonConvert.SerializeObject(requestData);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                string url = $"{_midtransBaseUrl}/charge";
                var response = await client.PostAsync(url, httpContent);
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseString);

                if (response.IsSuccessStatusCode)
                {
                    if (type == "echannel")
                    {
                        // Mandiri return: Biller Code - Bill Key
                        return $"{jsonResponse.biller_code} - {jsonResponse.bill_key}";
                    }
                    else
                    {
                        // Bank Lain: VA Number
                        if (jsonResponse.va_numbers != null)
                            return jsonResponse.va_numbers[0].va_number.ToString();
                        else if (jsonResponse.permata_va_number != null)
                            return jsonResponse.permata_va_number.ToString();
                    }
                    throw new Exception("Nomor VA tidak ditemukan di response.");
                }
                else
                {
                    string errMsg = jsonResponse.status_message ?? response.ReasonPhrase;
                    throw new Exception($"Midtrans Error: {errMsg}");
                }
            }
        }

        private void ShowSuccessPopup(string bank, string vaNumber, decimal amount)
        {
            var popup = new PaymentResult(bank, vaNumber, $"Rp {amount:N0}");
            popup.ShowDialog();

            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private decimal ParseCurrency(string currencyString)
        {
            if (string.IsNullOrEmpty(currencyString)) return 0;
            string clean = currencyString.Replace("Rp", "").Replace(".", "").Replace(" ", "").Trim();
            if (decimal.TryParse(clean, out decimal result)) return result;
            return 0;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
        }
    }
}