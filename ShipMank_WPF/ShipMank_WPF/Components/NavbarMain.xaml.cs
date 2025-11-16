using ShipMank_WPF.Pages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShipMank_WPF.Components
{
    public partial class NavbarMain : UserControl
    {
        public NavbarMain()
        {
            InitializeComponent();
        }

        private void SetActiveButton(Button activeButton)
        {
            // Reset semua button
            var buttons = new[] { HomeButton, HelpButton };

            foreach (var button in buttons)
            {
                button.Background = new SolidColorBrush(Colors.Transparent);
                button.Foreground = new SolidColorBrush(Colors.Black);
            }

            // Set button aktif
            activeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E90FF"));
            activeButton.Foreground = new SolidColorBrush(Colors.White);
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

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(HomeButton);
        }

        //private void RentalsButton_Click(object sender, RoutedEventArgs e)
        //{
        //    SetActiveButton(RentalsButton);
        //}

        //private void TicketsButton_Click(object sender, RoutedEventArgs e)
        //{
        //    SetActiveButton(TicketsButton);
        //}

        //private void OrdersButton_Click(object sender, RoutedEventArgs e)
        //{
        //    SetActiveButton(OrdersButton);
        //}

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(HelpButton);
        }
    }
}