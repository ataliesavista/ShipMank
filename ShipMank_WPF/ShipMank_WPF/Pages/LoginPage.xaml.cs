using System.Windows;
using System.Windows.Controls;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using ShipMank_WPF.Models;
using Google.Apis.Util.Store;
using System.Threading;
using System;
using System.Windows.Input;

namespace ShipMank_WPF.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        // ----------------------------------------
        // GOOGLE LOGIN ONLY
        // ----------------------------------------
        private async void GoogleLogin_Click(object sender, RoutedEventArgs e)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string clientId = configuration["GoogleAuth:ClientId"];
            string clientSecret = configuration["GoogleAuth:ClientSecret"];

            string[] scopes =
            {
                Oauth2Service.Scope.UserinfoEmail,
                Oauth2Service.Scope.UserinfoProfile
            };

            try
            {
                UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret
                    },
                    scopes,
                    "google_user",
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

                    // -------------------------
                    // CEK USER DI DATABASE
                    // -------------------------
                    User existingUser = User.GetUserByEmail(profile.Email);

                    if (existingUser != null)
                    {
                        existingUser.IsGoogleLogin = true;

                        Window parentWindow = Window.GetWindow(this);
                        if (parentWindow is MainWindow mw)
                        {
                            mw.CurrentUser = existingUser;
                            mw.ClosePopup();
                            mw.ShowLoggedInState();
                        }
                    }
                    else
                    {
                        MessageBox.Show(
                            $"Email {profile.Email} belum terdaftar.\nSilakan hubungi admin.",
                            "Akses Ditolak",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Login Google Gagal:\n{ex.Message}",
                    "Kesalahan",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
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
