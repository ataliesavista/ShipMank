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
using Google.Apis.Auth.OAuth2; // Diperlukan
using Google.Apis.Auth.OAuth2.Flows; // Diperlukan
using Google.Apis.Oauth2.v2; // Diperlukan
using Google.Apis.Oauth2.v2.Data; // Diperlukan
using Google.Apis.Services; // Diperlukan
using Google.Apis.Util.Store; // Diperlukan
using Microsoft.Extensions.Configuration; // Diperlukan

namespace ShipMank_WPF.Components
{
    public partial class SettingsControl : UserControl
    {
        // Flag untuk melacak mode Edit/Save
        private bool isEditing = false;

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

            // 2. Ambil data profil Google (sama seperti di Navbar)
            try
            {
                IConfiguration configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                string clientId = configuration["GoogleAuth:ClientId"];
                string clientSecret = configuration["GoogleAuth:ClientSecret"];
                string[] scopes = { Oauth2Service.Scope.UserinfoEmail, Oauth2Service.Scope.UserinfoProfile };

                UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
                    scopes,
                    "temp_user_id", // Pastikan ini SAMA dengan di file lain
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
                    EmailTextBox.Text = profile.Email;

                    // Data lain tidak disediakan oleh Google, biarkan default
                    GenderComboBox.SelectedIndex = 0; // Default ke "Wanita"
                    CityTextBox.Text = "Belum diatur"; // Placeholder
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat profil: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                FullNameTextBox.Text = "Error memuat data";
                EmailTextBox.Text = "Error memuat data";
            }
        }

        private void PopulateDateComboBoxes()
        {
            // Isi Hari
            CmbBirthDay.Items.Add("Hari");
            for (int i = 1; i <= 31; i++)
            {
                CmbBirthDay.Items.Add(i.ToString());
            }
            CmbBirthDay.SelectedIndex = 0;

            // Isi Bulan
            CmbBirthMonth.Items.Add("Bulan");
            string[] monthNames = DateTimeFormatInfo.CurrentInfo.MonthNames;
            foreach (string month in monthNames)
            {
                if (!string.IsNullOrEmpty(month))
                {
                    CmbBirthMonth.Items.Add(month);
                }
            }
            CmbBirthMonth.SelectedIndex = 0;

            // Isi Tahun (misal: dari 100 tahun lalu sampai tahun ini)
            CmbBirthYear.Items.Add("Tahun");
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear; i >= currentYear - 100; i--)
            {
                CmbBirthYear.Items.Add(i.ToString());
            }
            CmbBirthYear.SelectedIndex = 0;
        }

        private void EditSaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Balikkan status editing
            isEditing = !isEditing;

            // Terapkan mode baru
            SetEditMode(isEditing);

            // Jika BUKAN lagi mode editing (artinya tombol "Save" baru saja diklik)
            if (!isEditing)
            {
                // Di sinilah Anda akan menambahkan logika untuk menyimpan ke database
                MessageBox.Show("Perubahan telah disimpan (simulasi).", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SetEditMode(bool isEnabled)
        {
            // Kunci/buka kunci semua bidang
            FullNameTextBox.IsReadOnly = !isEnabled;
            EmailTextBox.IsReadOnly = !isEnabled; // Email biasanya tidak boleh diedit, tapi saya ikut permintaan
            CityTextBox.IsReadOnly = !isEnabled;

            GenderComboBox.IsEnabled = isEnabled;
            CmbBirthDay.IsEnabled = isEnabled;
            CmbBirthMonth.IsEnabled = isEnabled;
            CmbBirthYear.IsEnabled = isEnabled;

            // Ubah teks tombol
            EditSaveButton.Content = isEnabled ? "Save" : "Edit";
        }
    }
}