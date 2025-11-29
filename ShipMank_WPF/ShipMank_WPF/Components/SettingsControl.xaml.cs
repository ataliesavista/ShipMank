using System;
using System.Collections.Generic;
using System.Globalization; // Untuk nama bulan
using System.Linq;
using System.Text;
using System.Threading; // Untuk CancellationToken
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;

namespace ShipMank_WPF.Components
{
    public partial class SettingsControl : UserControl
    {
        // Flag untuk melacak mode Edit/Save Personal Info
        private bool isEditing = false;
        public event EventHandler DeleteAccountRequested;

        public SettingsControl()
        {
            InitializeComponent();
            PopulateDateComboBoxes();

            // Menghubungkan event Loaded
            this.Loaded += SettingsControl_Loaded;
        }

        private async void SettingsControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Atur form ke mode ReadOnly saat pertama kali dimuat
            SetEditMode(false);

            // 2. Ambil data profil Google
            try
            {
                // Cek apakah file konfigurasi ada sebelum build (Opsional, untuk safety)
                var builder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                IConfiguration configuration = builder.Build();

                string clientId = configuration["GoogleAuth:ClientId"];
                string clientSecret = configuration["GoogleAuth:ClientSecret"];

                // Jika config belum diset, skip Google Auth (agar tidak crash saat development)
                if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
                {
                    string[] scopes = { Oauth2Service.Scope.UserinfoEmail, Oauth2Service.Scope.UserinfoProfile };

                    UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
                        scopes,
                        "temp_user_id",
                        CancellationToken.None,
                        new FileDataStore("ShipMank.GoogleAuthStore")
                    );

                    if (credential != null)
                    {
                        var oauthService = new Oauth2Service(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = credential,
                            ApplicationName = "ShipMank_WPF",
                        });

                        Userinfo profile = await oauthService.Userinfo.Get().ExecuteAsync();

                        // 3. Isi data ke UI
                        FullNameTextBox.Text = profile.Name;
                        EmailTextBox.Text = profile.Email; // Email utama

                        // Default values
                        GenderComboBox.SelectedIndex = 0;
                    }
                }
                else
                {
                    // Dummy data jika tidak ada Google Auth config
                    FullNameTextBox.Text = "User Local";
                    EmailTextBox.Text = "user@local.com";
                }

                // ============================================================
                // MODIFIKASI: Logic City (Kosongkan jika tidak ada data)
                // ============================================================
                //string cityData = null; // Anggap data dari database kosong

                //if (string.IsNullOrEmpty(cityData))
                //{
                //    CityTextBox.Text = ""; // Dibuat kosong sesuai permintaan
                //}
                //else
                //{
                //    CityTextBox.Text = cityData;
                //}
            }
            catch (Exception ex)
            {
                // Log error tapi jangan crash aplikasi
                // MessageBox.Show($"Gagal memuat profil: {ex.Message}...");
                FullNameTextBox.Text = "Offline User";
            }
        }

        private void PopulateDateComboBoxes()
        {
            // Isi Hari
            CmbBirthDay.Items.Add("Hari");
            for (int i = 1; i <= 31; i++) CmbBirthDay.Items.Add(i.ToString());
            CmbBirthDay.SelectedIndex = 0;

            // Isi Bulan
            CmbBirthMonth.Items.Add("Bulan");
            string[] monthNames = DateTimeFormatInfo.CurrentInfo.MonthNames;
            foreach (string month in monthNames)
            {
                if (!string.IsNullOrEmpty(month)) CmbBirthMonth.Items.Add(month);
            }
            CmbBirthMonth.SelectedIndex = 0;

            // Isi Tahun
            CmbBirthYear.Items.Add("Tahun");
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear; i >= currentYear - 100; i--) CmbBirthYear.Items.Add(i.ToString());
            CmbBirthYear.SelectedIndex = 0;
        }

        // ============================================================
        // LOGIC: PERSONAL INFO EDIT
        // ============================================================
        private void EditSaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Balikkan status editing
            isEditing = !isEditing;

            // Terapkan mode baru
            SetEditMode(isEditing);

            // Jika tombol "Save" baru saja diklik
            if (!isEditing)
            {
                // Simpan ke database logic di sini
                MessageBox.Show("Personal information updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SetEditMode(bool isEnabled)
        {
            // Kunci/buka kunci semua bidang Personal Info
            FullNameTextBox.IsReadOnly = !isEnabled;

            // Note: Email biasanya dikelola terpisah (sesuai permintaan fitur tombol +Email)
            // Jadi EmailTextBox di bagian Personal Info/Account Info kita biarkan ReadOnly 
            // atau ikut logic ini jika fieldnya sama.

            //CityTextBox.IsReadOnly = !isEnabled;

            GenderComboBox.IsEnabled = isEnabled;
            CmbBirthDay.IsEnabled = isEnabled;
            CmbBirthMonth.IsEnabled = isEnabled;
            CmbBirthYear.IsEnabled = isEnabled;

            // Ubah tampilan tombol Edit/Save
            if (isEnabled)
            {
                EditSaveButton.Content = "Save";
                EditSaveButton.Background = (Brush)new BrushConverter().ConvertFrom("#10B981"); // Hijau
                EditSaveButton.Foreground = Brushes.White;
            }
            else
            {
                EditSaveButton.Content = "Edit";
                // Mengambil resource warna biru dari XAML
                EditSaveButton.Background = Brushes.White;
                EditSaveButton.Foreground = (Brush)FindResource("PrimaryBlueBrush");
            }
        }

        // ============================================================
        // MODIFIKASI: LOGIC EMAIL (+ EMAIL, SAVE, CANCEL)
        // ============================================================
        //private void BtnAddEmail_Click(object sender, RoutedEventArgs e)
        //{
        //    // 1. Sembunyikan tombol "+ Email"
        //    BtnAddEmail.Visibility = Visibility.Collapsed;

        //    // 2. Munculkan tombol Save & Cancel
        //    StpEmailActions.Visibility = Visibility.Visible;

        //    // 3. Munculkan Input Email Baru
        //    StpNewEmailInput.Visibility = Visibility.Visible;
        //    TxtNewEmail.Text = ""; // Reset input
        //    TxtNewEmail.Focus();
        //}

        //private void BtnCancelEmail_Click(object sender, RoutedEventArgs e)
        //{
        //    ResetEmailUI();
        //}

        //private void BtnSaveEmail_Click(object sender, RoutedEventArgs e)
        //{
        //    // Validasi dan Simpan Email Baru
        //    string newEmail = TxtNewEmail.Text;

        //    if (!string.IsNullOrWhiteSpace(newEmail) && newEmail.Contains("@"))
        //    {
        //        // Update Tampilan Email Utama (Simulasi)
        //        EmailTextBox.Text = newEmail;

        //        // TODO: Update ke Database/Google Account di sini

        //        MessageBox.Show("Email updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        //        ResetEmailUI();
        //    }
        //    else
        //    {
        //        MessageBox.Show("Please enter a valid email address.", "Invalid Email", MessageBoxButton.OK, MessageBoxImage.Warning);
        //    }
        //}

        private void ResetEmailUI()
        {
            //// Kembalikan ke kondisi awal
            //BtnAddEmail.Visibility = Visibility.Visible;
            //StpEmailActions.Visibility = Visibility.Collapsed;
            //StpNewEmailInput.Visibility = Visibility.Collapsed;
        }

        // ============================================================
        // MODIFIKASI: LOGIC DELETE ACCOUNT POPUP
        // ============================================================

        // 1. Saat tombol "Delete" di Section diklik -> Buka Popup
        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            DeleteAccountRequested?.Invoke(this, EventArgs.Empty);
        }

    }
}