using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using ShipMank_WPF.Models;
using System.Xml.Linq;
using Google.Apis.Oauth2.v2;

namespace ShipMank_WPF.Pages
{
    public partial class SignUp : Page
    {
        public SignUp()
        {
            InitializeComponent();
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            // Input hanya Username, Password, dan Email
            string username = tbUsername.Text.Trim(); // Tambahkan Trim() untuk kebersihan
            string password = tbPassword.Password;
            string email = tbEmail.Text.Trim(); // Tambahkan Trim() untuk kebersihan

            // Tambahkan validasi dasar (misal: cek field kosong)
            // MODIFIKASI: Hanya cek 3 field wajib (Username, Password, Email)
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Mohon isi Username, Email, dan Password.", "Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Panggil metode Register dari Model User. 
            // Parameter 'name' tidak diisi, sehingga menggunakan default (null).
            if (new User().Register(username, password, email)) // Gunakan 'new User()' karena Register bukan static
            {
                MessageBox.Show("Akun berhasil dibuat. Silakan Login.", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);

                // Navigasi ke LoginPage
                Window parentWindow = Window.GetWindow(this);
                if (parentWindow is MainWindow mw)
                {
                    mw.ShowPopup(new LoginPage());
                }
            }
            else
            {
                MessageBox.Show("Registrasi gagal. Username atau Email mungkin sudah terdaftar.", "Gagal", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoginText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow is MainWindow mw)
            {
                mw.ShowPopup(new LoginPage());
            }
        }

        private async void GoogleSignUp_Click(object sender, RoutedEventArgs e)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string clientId = configuration["GoogleAuth:ClientId"];
            string clientSecret = configuration["GoogleAuth:ClientSecret"];

            string[] scopes = { "email", "profile" };

            try
            {
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

                if (credential != null)
                {
                    var oauthService = new Oauth2Service(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "ShipMank_WPF",
                    });

                    Userinfo profile = await oauthService.Userinfo.Get().ExecuteAsync();

                    // ----------------------------------------------------
                    // INTEGRASI DATABASE GOOGLE SIGNUP
                    // ----------------------------------------------------

                    // Cek apakah user sudah terdaftar di DB (melalui email)
                    User existingUser = User.GetUserByEmail(profile.Email);

                    if (existingUser != null)
                    {
                        // ... (Logika User sudah ada diabaikan) ...
                    }
                    else
                    {
                        // MODIFIKASI: Gunakan Name dari Google sebagai nilai opsional
                        bool success = new User().Register(
                            username: profile.Email,
                            passwordRaw: Guid.NewGuid().ToString(),
                            email: profile.Email,
                            name: profile.Name // Name dari Google diisi, sisanya NULL
                        );

                        if (success)
                        {
                            // ... (Logika Sukses diabaikan) ...
                        }
                        else
                        {
                            // ... (Logika Gagal diabaikan) ...
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ... (Error handling diabaikan) ...
            }
        }
    }
}