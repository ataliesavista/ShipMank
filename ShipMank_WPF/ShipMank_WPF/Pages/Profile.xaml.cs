using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects; // Penting untuk BlurEffect
using ShipMank_WPF.Components;

namespace ShipMank_WPF.Pages
{
    public partial class Profile : Page
    {
        private SettingsControl settingsPage;
        private PassengersControl passengersPage;

        public Profile()
        {
            InitializeComponent();

            // Inisialisasi UserControl
            settingsPage = new SettingsControl();
            passengersPage = new PassengersControl();

            // Hubungkan event navigasi NavBar
            SideNavBar.NavigateMyAccount += OnNavigateMyAccount;
            SideNavBar.NavigatePassengerList += OnNavigatePassengerList;

            // ====================================================================
            // TAMBAHAN: Hubungkan event Delete dari settingsPage ke logic Popup di sini
            // ====================================================================
            settingsPage.DeleteAccountRequested += SettingsPage_DeleteAccountRequested;

            // Default Page
            MainContent.Content = settingsPage;
        }

        // 1. Saat tombol Delete di SettingsControl diklik
        private void SettingsPage_DeleteAccountRequested(object sender, System.EventArgs e)
        {
            // Terapkan Blur ke Main Profile Content (termasuk Sidebar)
            BlurEffect blur = new BlurEffect();
            blur.Radius = 20;
            MainProfileContent.Effect = blur;

            // Munculkan Popup Overlay
            DeleteConfirmationOverlay.Visibility = Visibility.Visible;
        }

        // 2. Saat tombol Cancel Popup diklik
        private void CancelDelete_Click(object sender, RoutedEventArgs e)
        {
            // Hilangkan Blur
            MainProfileContent.Effect = null;

            // Sembunyikan Popup
            DeleteConfirmationOverlay.Visibility = Visibility.Collapsed;
        }

        // 3. Saat tombol Confirm Delete diklik
        private void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            // Lakukan logika penghapusan akun di sini (Database, API, dll)
            MessageBox.Show("Account deleted successfully from Profile Page.");
            MainProfileContent.Effect = null;
            DeleteConfirmationOverlay.Visibility = Visibility.Collapsed;
            NavigationService.Navigate(new Home2());
        }

        // Navigasi (Tetap sama)
        private void OnNavigateMyAccount(object sender, RoutedEventArgs e)
        {
            MainContent.Content = settingsPage;
        }

        private void OnNavigatePassengerList(object sender, RoutedEventArgs e)
        {
            MainContent.Content = passengersPage;
        }
    }
}