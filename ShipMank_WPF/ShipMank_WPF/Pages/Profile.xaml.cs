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
using ShipMank_WPF.Components;

namespace ShipMank_WPF.Pages
{
    /// <summary>
    /// Interaction logic for Profile.xaml
    /// </summary>
    public partial class Profile : Page
    {
        // 1. Buat instance dari UserControl yang akan diganti-ganti
        private SettingsControl settingsPage;
        private PassengersControl passengersPage; // Asumsikan Anda akan membuat file ini

        public Profile()
        {
            InitializeComponent();

            // 2. Inisialisasi UserControl
            settingsPage = new SettingsControl();
            passengersPage = new PassengersControl(); // Buat file ini

            // 3. Hubungkan event dari 'SideNavBar' ke method di file ini
            SideNavBar.NavigateMyAccount += OnNavigateMyAccount;
            SideNavBar.NavigatePassengerList += OnNavigatePassengerList;

            // 4. Atur halaman default saat Profile.xaml pertama kali dimuat
            // Karena "My Account" IsChecked=True, kita tampilkan SettingsControl
            MainContent.Content = settingsPage;
        }

        // Method ini akan dijalankan saat 'NavigateMyAccount' (dari NavBar) di-klik
        private void OnNavigateMyAccount(object sender, RoutedEventArgs e)
        {
            MainContent.Content = settingsPage;
        }

        // Method ini akan dijalankan saat 'NavigatePassengerList' (dari NavBar) di-klik
        private void OnNavigatePassengerList(object sender, RoutedEventArgs e)
        {
            MainContent.Content = passengersPage;
        }
    }
}
