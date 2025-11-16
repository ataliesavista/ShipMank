using ShipMank_WPF.Pages;
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

namespace ShipMank_WPF.Components
{
    /// <summary>
    /// Interaction logic for NavbarDash.xaml
    /// </summary>
    public partial class NavbarDash : UserControl
    {
        public NavbarDash()
        {
            InitializeComponent();
        }

        private void SetActiveButton(Button activeButton)
        {
            var buttons = new[] { HomeButton, RentalsButton, TicketsButton, OrdersButton, HelpButton, ProfileButton };
            foreach (var button in buttons)
            {
                if (button != null)
                {
                    button.Background = new SolidColorBrush(Colors.Transparent);
                    button.Foreground = new SolidColorBrush(Colors.Black);
                }
            }

            if (activeButton != null)
            {
                activeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E90FF"));
                activeButton.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(HomeButton);
        }

        private void RentalsButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(RentalsButton);
        }

        private void TicketsButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(TicketsButton);
        }

        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(OrdersButton);
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(HelpButton);
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(ProfileButton);

            var main = (MainWindow)Application.Current.MainWindow;
            main.MainFrame.Content = new Profile();
        }


    }
}
