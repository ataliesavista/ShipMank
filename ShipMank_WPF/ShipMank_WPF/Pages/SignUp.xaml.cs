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
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;

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
            MessageBox.Show("Akun berhasil dibuat", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);

            Window parentWindow = Window.GetWindow(this);
            if (parentWindow is MainWindow mw)
            {
                mw.ShowPopup(new LoginPage());
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
                    MessageBox.Show($"Sign Up Google Berhasil! Pengguna: {credential.UserId}",
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