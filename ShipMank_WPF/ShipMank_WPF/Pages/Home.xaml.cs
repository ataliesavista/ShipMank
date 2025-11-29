using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        public Home()
        {
            InitializeComponent();
            TicketDatePicker.SelectedDate = DateTime.Now;
            UpdateDateDisplay();

            TicketDatePicker.SelectedDateChanged += TicketDatePicker_SelectedDateChanged;
        }

        private void DateBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TicketDatePicker.IsDropDownOpen = true;
        }

        private void TicketDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDateDisplay();
        }

        private void UpdateDateDisplay()
        {
            if (TicketDatePicker.SelectedDate.HasValue)
            {
                var date = TicketDatePicker.SelectedDate.Value;
                var culture = new CultureInfo("id-ID");

                string dayOfWeek = date.ToString("ddd", culture);
                string day = date.Day.ToString("00");
                string month = date.ToString("MMM", culture); 
                string year = date.ToString("yy");

                DateTextBlock.Text = $"{dayOfWeek}, {day} {month} {year}";
            }
        }

        private void TicketsTab_Click(object sender, RoutedEventArgs e)
        {
            TicketsTabButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#154D71"));
            TicketsTabButton.Foreground = new SolidColorBrush(Colors.White);

            RentalsTabButton.Background = new SolidColorBrush(Colors.Transparent);
            RentalsTabButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888"));

            TicketsContent.Visibility = Visibility.Visible;
            RentalsContent.Visibility = Visibility.Collapsed;
        }

        private void RentalsTab_Click(object sender, RoutedEventArgs e)
        {
            RentalsTabButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#154D71"));
            RentalsTabButton.Foreground = new SolidColorBrush(Colors.White);

            TicketsTabButton.Background = new SolidColorBrush(Colors.Transparent);
            TicketsTabButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888"));

            RentalsContent.Visibility = Visibility.Visible;
            TicketsContent.Visibility = Visibility.Collapsed;
        }
    }
}
