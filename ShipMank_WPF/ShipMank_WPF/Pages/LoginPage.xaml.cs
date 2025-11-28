using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks; // Pastikan ini ada
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using ShipMank_WPF.Models;

namespace ShipMank_WPF.Pages
{
    public partial class LoginPage : Page
    {
        private bool _isLoggingIn = false;

        public LoginPage()
        {
            InitializeComponent();
        }

        private async void GoogleLogin_Click(object sender, RoutedEventArgs e)
        {
            // Mencegah klik ganda
            if (_isLoggingIn) return;
            _isLoggingIn = true;

            // Disable button visual
            if (sender is Button loginButton) loginButton.IsEnabled = false;

            try
            {
                IConfiguration configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                string clientId = configuration["GoogleAuth:ClientId"];
                string clientSecret = configuration["GoogleAuth:ClientSecret"];

                string[] scopes = {
                    Oauth2Service.Scope.UserinfoEmail,
                    Oauth2Service.Scope.UserinfoProfile
                };

                string tokenPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ShipMank.GoogleAuthStore"
                );

                // Gunakan Timeout agar tidak hang selamanya jika user menutup browser
                using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2)))
                {
                    // 1. AUTHORIZE
                    // Library akan otomatis membuka browser jika token tidak ada di folder.
                    // Jika token ada, dia akan langsung return credential tanpa buka browser.
                    UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        new ClientSecrets
                        {
                            ClientId = clientId,
                            ClientSecret = clientSecret
                        },
                        scopes,
                        "google_user",
                        cts.Token,
                        new FileDataStore(tokenPath, true)
                    );

                    if (credential != null)
                    {
                        // --- BAGIAN INI DIHAPUS UNTUK MENCEGAH LOGIN 2X ---
                        // if (credential.Token.IsExpired(credential.Flow.Clock)) { ... } 
                        // ---------------------------------------------------

                        // 2. AKSES API
                        // Jika token expired, baris di bawah ini otomatis akan refresh token
                        // tanpa membuka browser lagi.
                        var oauthService = new Oauth2Service(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = credential,
                            ApplicationName = "ShipMank_WPF",
                        });

                        Userinfo profile = await oauthService.Userinfo.Get().ExecuteAsync();

                        // 3. LOGIKA DATABASE
                        User existingUser = User.GetUserByEmail(profile.Email);

                        if (existingUser != null)
                        {
                            // Login Berhasil
                            existingUser.IsGoogleLogin = true;

                            Window parentWindow = Window.GetWindow(this);
                            if (parentWindow is MainWindow mw)
                            {
                                mw.CurrentUser = existingUser;
                                mw.ClosePopup();
                                mw.ShowLoggedInState();

                                MessageBox.Show(
                                    $"Selamat datang, {existingUser.Name}!",
                                    "Login Berhasil",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information
                                );
                            }
                        }
                        else
                        {
                            // User tidak terdaftar -> Hapus Token agar bisa login ulang dengan akun lain
                            // RevokeTokenAsync menghapus izin di sisi Google & Lokal
                            await credential.RevokeTokenAsync(CancellationToken.None);

                            // Opsional: Hapus folder lokal juga untuk kebersihan
                            // (Mencegah credential.json tertinggal)
                            if (Directory.Exists(tokenPath))
                            {
                                try { Directory.Delete(tokenPath, true); } catch { }
                            }

                            MessageBox.Show(
                                $"Email {profile.Email} belum terdaftar.\nSilakan Sign Up atau hubungi admin.",
                                "Akses Ditolak",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning
                            );

                            Window parentWindow = Window.GetWindow(this);
                            if (parentWindow is MainWindow mw)
                            {
                                mw.ClosePopup();
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Login dibatalkan (Timeout).", "Info");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login Google Gagal:\n{ex.Message}", "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isLoggingIn = false;
                if (sender is Button btn) btn.IsEnabled = true;
            }
        }

        private void SignUpText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow is MainWindow mw)
            {
                mw.ShowPopup(new SignUp());
            }
        }
    }
}