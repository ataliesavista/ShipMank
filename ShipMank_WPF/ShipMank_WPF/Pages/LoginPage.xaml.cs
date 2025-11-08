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
                Window parentWindow = Window.GetWindow(this);
                if (parentWindow is MainWindow mw)
                {
                    mw.ClosePopup();
                    mw.ShowLoggedInState();
                }
            }
            else
            {
                MessageBox.Show("Username atau password salah", "Login Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
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