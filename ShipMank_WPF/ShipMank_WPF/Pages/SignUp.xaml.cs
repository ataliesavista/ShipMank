using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using ShipMank_WPF.Models;

namespace ShipMank_WPF.Pages
{
    public partial class SignUp : Page
    {
        public SignUp()
        {
            InitializeComponent();
        }

        // ================================================
        // TEKS "Masuk" — Navigasi ke login page
        // ================================================
        private void LoginText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow is MainWindow mw)
            {
                mw.ShowPopup(new LoginPage());
            }
        }

        // ================================================
        //          GOOGLE SIGN UP ONLY
        // ================================================
        private async void GoogleSignUp_Click(object sender, RoutedEventArgs e)
        {
            // Ambil konfigurasi clientId dan secret
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string clientId = configuration["GoogleAuth:ClientId"];
            string clientSecret = configuration["GoogleAuth:ClientSecret"];

            string[] scopes = { "email", "profile" };

            try
            {
                // Proses OAuth Google
                UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret
                    },
                    scopes,
                    "temp_user_id",
                    CancellationToken.None,
                    new FileDataStore("ShipMank.GoogleAuthStore")
                );

                if (credential == null)
                {
                    MessageBox.Show("Gagal menghubungkan ke Google.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Ambil info profil Google
                var oauthService = new Oauth2Service(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "ShipMank_WPF",
                });

                Userinfo profile = await oauthService.Userinfo.Get().ExecuteAsync();

                if (profile == null || string.IsNullOrEmpty(profile.Email))
                {
                    MessageBox.Show("Tidak dapat mengambil data Google.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // ============================================================
                //          CEK APAKAH USER SUDAH TERDAFTAR
                // ============================================================
                User existingUser = User.GetUserByEmail(profile.Email);

                if (existingUser != null)
                {
                    // User sudah ada → Arahkan ke login
                    MessageBox.Show(
                        "Email Google ini sudah terdaftar. Silakan login.",
                        "Akun Ditemukan",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    Window parentWindow = Window.GetWindow(this);
                    if (parentWindow is MainWindow mw)
                    {
                        mw.ShowPopup(new LoginPage());
                    }

                    return;
                }

                // ============================================================
                //      AKUN BELUM ADA → BUAT AKUN BARU OTOMATIS
                // ============================================================
                bool success = new User().Register(
                    username: profile.Email,          // Username = email Google
                    passwordRaw: Guid.NewGuid().ToString(),  // Random password (karena tidak digunakan)
                    email: profile.Email,
                    name: profile.Name
                );

                if (success)
                {
                    MessageBox.Show(
                        "Akun berhasil dibuat melalui Google. Silakan login.",
                        "Sukses",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    Window parentWindow = Window.GetWindow(this);
                    if (parentWindow is MainWindow mw)
                    {
                        mw.ShowPopup(new LoginPage());
                    }
                }
                else
                {
                    MessageBox.Show(
                        "Gagal membuat akun baru menggunakan Google.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Terjadi kesalahan:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
