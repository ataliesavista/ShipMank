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
            string username = tbUsername.Text;
            string password = tbPassword.Password;

            if (username == "admin" && password == "1234")
            {
                Window parentWindow = Window.GetWindow(this);
                if (parentWindow is MainWindow mw)
                {
                    mw.ClosePopup();
                    mw.ShowLoggedInState();
                }
            }
            else
            {
                MessageBox.Show("Username atau password salah", "Login Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        ApplicationName = applicationName,
                    });

                    Userinfo profile = await oauthService.Userinfo.Get().ExecuteAsync();

                    MessageBox.Show($"Login Google Berhasil!\nNama: {profile.Name}\nEmail: {profile.Email}",
                                    "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);

                    Window parentWindow = Window.GetWindow(this);
                    if (parentWindow is MainWindow mw)
                    {
                        mw.ClosePopup();
                        mw.ShowLoggedInState();
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