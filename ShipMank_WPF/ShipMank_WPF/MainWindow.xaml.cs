using ShipMank_WPF.Pages;
using ShipMank_WPF.Components;
using ShipMank_WPF.Models; 
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Effects;
using System.Windows.Input;

namespace ShipMank_WPF
{
    public partial class MainWindow : Window
    {
        // <--- TAMBAHAN PENTING 2: Properti untuk menyimpan User yang Login
        public User CurrentUser { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ShowLoggedOutState();
        }

        public void ShowLoggedOutState()
        {
            // Saat logout, hapus data user
            CurrentUser = null;

            NavbarContainer.Content = new NavbarMain();
            MainFrame.Navigate(new Home2());
        }

        public void ShowLoggedInState()
        {
            NavbarContainer.Content = new NavbarDash();
            MainFrame.Navigate(new BeliTiket());

            if (CurrentUser != null && CurrentUser.IsGoogleLogin)
            {

            }
            else
            {
                
            }
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