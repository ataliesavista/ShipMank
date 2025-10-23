using ShipMank_WPF.Pages;
using System.Windows;
using System.Windows.Controls;

namespace ShipMank_WPF.Components
{
    public partial class NavbarMain : UserControl
    {
        public NavbarMain()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow is MainWindow mw)
            {
                mw.ShowPopup(new LoginPage());
            }
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow is MainWindow mw)
            {
                mw.ShowPopup(new SignUp());
            }
        }
    }
}