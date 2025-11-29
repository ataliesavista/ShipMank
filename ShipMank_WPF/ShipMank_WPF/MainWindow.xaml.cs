using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;
using ShipMank_WPF.Components;
using ShipMank_WPF.Models;
using ShipMank_WPF.Pages;

namespace ShipMank_WPF
{
    public partial class MainWindow : Window
    {
        public User CurrentUser { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ShowInitialState();
        }

        public void ShowInitialState()
        {
            CurrentUser = null;
            if (MainFrame.NavigationService != null && MainFrame.NavigationService.CanGoBack)
            {
                MainFrame.NavigationService.RemoveBackEntry();
            }
            MainFrame.Content = null;

            NavbarContainer.Content = new NavbarMain();
            MainFrame.Navigate(new Home2());
        }

        public void Logout()
        {
            CurrentUser = null;
            DeleteGoogleToken();
            if (MainFrame.NavigationService != null && MainFrame.NavigationService.CanGoBack)
            {
                MainFrame.NavigationService.RemoveBackEntry();
            }
            MainFrame.Content = null;

            NavbarContainer.Content = new NavbarMain();
            MainFrame.Navigate(new Home2());

            MessageBox.Show(
                "Anda telah berhasil logout.",
                "Logout",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void DeleteGoogleToken()
        {
            try
            {
                string googleStorePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ShipMank.GoogleAuthStore"
                );

                if (Directory.Exists(googleStorePath))
                {
                    Directory.Delete(googleStorePath, true);
                    System.Diagnostics.Debug.WriteLine("Token Google berhasil dihapus");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Gagal menghapus token Google: {ex.Message}");
            }
        }

        public void ShowLoggedInState()
        {
            NavbarContainer.Content = new NavbarDash();
            MainFrame.Navigate(new BeliTiket());
        }

        public void ShowPopup(Page page)
        {
            MainContentGrid.Effect = new BlurEffect { Radius = 15 };
            PopupFrame.Navigate(page);
            PopupOverlay.Visibility = Visibility.Visible;
        }

        public void ClosePopup()
        {
            MainContentGrid.Effect = null;
            PopupOverlay.Visibility = Visibility.Collapsed;
            PopupFrame.Content = null;

            if (PopupFrame.CanGoBack)
            {
                PopupFrame.RemoveBackEntry();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ClosePopup();
        }

        private void PopupOverlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source == sender)
            {
                ClosePopup();
            }
        }
    }
}