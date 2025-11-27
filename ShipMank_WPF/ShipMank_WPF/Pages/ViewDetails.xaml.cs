using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ShipMank_WPF.Models; // Pastikan namespace OrderHistoryItem terjangkau (jika dipisah)
// Jika OrderHistoryItem ada di dalam History.xaml.cs (sebagai nested class public atau di file yang sama), 
// pastikan using ShipMank_WPF.Pages; juga ada.

namespace ShipMank_WPF.Pages
{
    public partial class ViewDetails : Page
    {
        // Konstruktor UTAMA yang menerima data dari History
        public ViewDetails(OrderHistoryItem historyItem)
        {
            InitializeComponent();

            // Set DataContext agar Binding di XAML berfungsi
            this.DataContext = historyItem;
        }

        // Konstruktor default (jika diperlukan oleh designer XAML, opsional tapi bagus untuk menghindari error designer)
        public ViewDetails()
        {
            InitializeComponent();
        }

        // Tombol Back (opsional, jika ada di UI)
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}