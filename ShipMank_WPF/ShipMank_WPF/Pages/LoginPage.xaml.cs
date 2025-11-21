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
using Google.Apis.Util.Store; 
using System; 
using Google.Apis.Auth.OAuth2.Flows; 
using Google.Apis.Auth.OAuth2.Responses; 
using Google.Apis.Oauth2.v2; 
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services; 
using Microsoft.Extensions.Configuration;
using ShipMank_WPF.Models;

namespace ShipMank_WPF.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Ambil input wajib: Username dan Password
            string username = tbUsername.Text;
            string password = tbPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Mohon isi Username dan Password.", "Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Panggil metode Login dari Model User
            User loggedInUser = User.Login(username, password);

            if (loggedInUser != null)
            {
                loggedInUser.IsGoogleLogin = false;
                MessageBox.Show($"Selamat datang!", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);

                // Lanjut ke Logged In State (Asumsi MainWindow memiliki metode untuk ini)
                Window parentWindow = Window.GetWindow(this);
                if (parentWindow is MainWindow mw)
                {
                    // Anda mungkin ingin menyimpan objek user ini di MainWindow atau ViewModel
                    mw.CurrentUser = loggedInUser;
                    mw.ClosePopup();
                    mw.ShowLoggedInState();
                }
            }
            else
            {
                MessageBox.Show("Login gagal. Username atau Password salah.", "Gagal", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SignUpText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Navigasi ke halaman Sign Up
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow is MainWindow mw)
            {
                mw.ShowPopup(new SignUp());
            }
        }

        private async void GoogleLogin_Click(object sender, RoutedEventArgs e)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string clientId = configuration["GoogleAuth:ClientId"];
            string clientSecret = configuration["GoogleAuth:ClientSecret"];

            string[] scopes = { Oauth2Service.Scope.UserinfoEmail, Oauth2Service.Scope.UserinfoProfile };
            string applicationName = "ShipMank_WPF";

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
                    // INTEGRASI DATABASE GOOGLE LOGIN
                    // ----------------------------------------------------

                    // 1. Cek apakah user sudah terdaftar di DB
                    User existingUser = User.GetUserByEmail(profile.Email);

                    if (existingUser != null)
                    {
                        // SET KE TRUE KARENA INI GOOGLE
                        existingUser.IsGoogleLogin = true;

                        MessageBox.Show($"Login Google Berhasil!...", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);

                        Window parentWindow = Window.GetWindow(this);
                        if (parentWindow is MainWindow mw)
                        {
                            mw.CurrentUser = existingUser; // Oper user
                            mw.ClosePopup();
                            mw.ShowLoggedInState();
                        }
                    }
                    else
                    {
                        // 3. User belum terdaftar (WAJIB SIGN UP DAHULU)
                        MessageBox.Show($"Email {profile.Email} belum terdaftar. Silakan Sign Up terlebih dahulu.",
                                        "Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);

                        // Opsional: Langsung arahkan ke halaman Sign Up
                        Window parentWindow = Window.GetWindow(this);
                        if (parentWindow is MainWindow mw)
                        {
                            mw.ShowPopup(new SignUp());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login Google Gagal: {ex.Message}",
                                "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}