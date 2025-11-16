using System.Collections.Generic;
using System.Windows.Controls;

namespace ShipMank_WPF.Pages
{
    /// <summary>
    /// Interaction logic for History.xaml
    /// </summary>
    public partial class History : Page
    {
        public History()
        {
            InitializeComponent();
            // Memuat data tiruan untuk mengisi DataGrid
            LoadMockData();
        }

        private void LoadMockData()
        {
            // Membuat list data
            var historyItems = new List<OrderHistoryItem>
            {
                new OrderHistoryItem { OrderID = "TCK00001", ItemName = "Kapal Van Der Wijck", Date = new System.DateTime(2024, 1, 15), Route = "Jakarta (MAK) - Surabaya (TPR)", TotalString = "IDR 10.000.000", Status = "Completed" },
                new OrderHistoryItem { OrderID = "RTL00001", ItemName = "Going Merry", Date = new System.DateTime(2025, 9, 30), Route = "Lombok, Senggigi", TotalString = "IDR 975.000", Status = "Upcoming" },
                new OrderHistoryItem { OrderID = "RTL00002", ItemName = "Kapal Babe Asep", Date = new System.DateTime(2023, 12, 12), Route = "Jakarta, Pulau Seribu", TotalString = "IDR 200.000", Status = "Cancelled" },
                new OrderHistoryItem { OrderID = "RTL00003", ItemName = "Kapal Babe Asep", Date = new System.DateTime(2023, 11, 1), Route = "Jakarta, Pulau Seribu", TotalString = "IDR 200.000", Status = "Unpaid" }
            };

            // Mengatur ItemsSource dari DataGrid ke list data
            HistoryDataGrid.ItemsSource = historyItems;
        }

        private void HistoryDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    /* * Kelas helper untuk data.
     * Idealnya, kelas ini ada di filenya sendiri (misalnya, di dalam folder 'Models'),
     * tapi saya letakkan di sini agar lebih mudah disalin.
     */
    public class OrderHistoryItem
    {
        public string OrderID { get; set; }
        public string ItemName { get; set; }
        public System.DateTime Date { get; set; }
        public string Route { get; set; }
        public string TotalString { get; set; } // Menggunakan string agar sesuai dengan format 'IDR ...'
        public string Status { get; set; }
    }
}