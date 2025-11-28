using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
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
using Google.Apis.Auth.OAuth2.Flows; 
using Google.Apis.Oauth2.v2; 
using Google.Apis.Oauth2.v2.Data; 
using Google.Apis.Services; 
using Microsoft.Extensions.Configuration; 


namespace ShipMank_WPF.Components
{
    public partial class ProfileNavbar : UserControl
    {
        public event RoutedEventHandler NavigatePassengerList;
        public event RoutedEventHandler NavigateMyAccount;

        public ProfileNavbar()
        {
            InitializeComponent();

            PassengerListButton.Click += (s, e) => NavigatePassengerList?.Invoke(this, e);
            MyAccountButton.Click += (s, e) => NavigateMyAccount?.Invoke(this, e);

            this.Loaded += ProfileNavbar_Loaded;
        }

        private async void ProfileNavbar_Loaded(object sender, RoutedEventArgs e)
        {
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

                    ProfileNameTextBlock.Text = profile.Name;
                    ProfilePictureImageBrush.ImageSource = new BitmapImage(new Uri(profile.Picture));
                }
            }
            catch (Exception ex)
            {
                ProfileNameTextBlock.Text = "Error";
                MessageBox.Show($"Gagal memuat profil: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private async void LogoutButton_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Hapus token Google yang tersimpan
                var dataStore = new FileDataStore("ShipMank.GoogleAuthStore", true);
                await dataStore.DeleteAsync<TokenResponse>("google_user");

                // Panggil method Logout dari MainWindow (bukan ShowLoggedOutState)
                var main = (MainWindow)Application.Current.MainWindow;
                main.Logout(); // <--- PERBAIKAN: Gunakan Logout() bukan ShowLoggedOutState()
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Gagal logout: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

    }
}