using ShipMank_WPF.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;

namespace ShipMank_WPF.Components
{
    public partial class NavbarDash : UserControl
    {
        public NavbarDash()
        {
            InitializeComponent();
        }

        private async void NavbarDash_Loaded(object sender, RoutedEventArgs e)
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

        private void SetActiveButton(Button activeButton)
        {
            var buttons = new[] { HomeButton, RentalsButton, OrdersButton, ProfileButton };
            foreach (var button in buttons)
            {
                if (button != null)
                {
                    if (button == ProfileButton)
                    {
                        button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9AF"));
                        button.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        button.Background = new SolidColorBrush(Colors.Transparent);
                        button.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }

            if (activeButton != null)
            {
                if (activeButton == ProfileButton)
                {
                    activeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0E68C")); // Warna lebih gelap sedikit
                }
                else
                {
                    activeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E90FF"));
                    activeButton.Foreground = new SolidColorBrush(Colors.White);
                }
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(HomeButton);
            var main = (MainWindow)Application.Current.MainWindow;
            main.MainFrame.Content = new Home2();
        }

        private void RentalsButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(RentalsButton);
            var main = (MainWindow)Application.Current.MainWindow;
            main.MainFrame.Content = new BeliTiket();
        }

        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(OrdersButton);
            var main = (MainWindow)Application.Current.MainWindow;
            main.MainFrame.Content = new History();
        }

        //private void HelpButton_Click(object sender, RoutedEventArgs e)
        //{
        //    SetActiveButton(HelpButton);
        //}

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(ProfileButton);
            var main = (MainWindow)Application.Current.MainWindow;
            main.MainFrame.Content = new Profile();
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dataStore = new FileDataStore("ShipMank.GoogleAuthStore");
                await dataStore.DeleteAsync<TokenResponse>("temp_user_id");

                MessageBox.Show("Anda telah berhasil logout.", "Logout Sukses", MessageBoxButton.OK, MessageBoxImage.Information);

                var main = (MainWindow)Application.Current.MainWindow;
                main.ShowLoggedOutState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal logout: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}