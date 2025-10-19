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

namespace ShipMank_WPF.Pages
{
    /// <summary>
    /// Interaction logic for SignUp.xaml
    /// </summary>
    public partial class SignUp : Page
    {
        public SignUp()
        {
            InitializeComponent();
        }
        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Akun berhasil dibuat 🎉", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SignUpText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Navigasi ke halaman LoginPage
            this.NavigationService?.Navigate(new LoginPage());
        }

    }
}
