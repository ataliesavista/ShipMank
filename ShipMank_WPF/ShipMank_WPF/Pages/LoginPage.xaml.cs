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
    /// Interaction logic for LoginPage.xaml
    /// </summary>
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
                MessageBox.Show("Login berhasil 🎉", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Username atau password salah 😢", "Login Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SignUpText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Navigasi ke halaman SignUp
            this.NavigationService?.Navigate(new SignUp());
        }

    }
}
