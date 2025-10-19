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
using ShipMank_WPF.Models;
using ShipMank_WPF.Pages;

namespace ShipMank_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new LoginPage());

            /*// Testing Code (pindahan dari Form1_Load)
            var speedboatType = new ShipType(1, ShipTypeEnum.Speedboat, "Kapal cepat untuk perjalanan singkat");
            var yachtType = new ShipType(2, ShipTypeEnum.Yacht, "Kapal mewah untuk rekreasi");

            var kapal1 = new Kapal(1, "Fastboat Express", speedboatType, 50, 250000, "Jakarta",
                                   "Kapal cepat dan nyaman", "AC, WiFi, Toilet");
            var kapal2 = new Kapal(2, "Luxury Yacht Bali", yachtType, 30, 500000, "Bali",
                                   "Yacht mewah dengan fasilitas lengkap", "AC, Bar, Restaurant, Pool");

            var allKapals = new List<Kapal> { kapal1, kapal2 };

            var user = new User(1, "johndoe", "password123", "john@email.com",
                                "John Doe", "08123456789", "Jl. Sudirman No. 1");

            // MessageBox sama seperti WinForms, tapi pakai System.Windows
            MessageBox.Show($"Login sukses? {user.Login("johndoe", "password123")}");
            MessageBox.Show(kapal1.TampilDetail(), "Detail Kapal");

            var searchResults = user.SearchKapal(allKapals, "Jakarta", maxPrice: 300000);
            MessageBox.Show($"Hasil pencarian: {searchResults.Count} kapal ditemukan", "Pencarian");

            var booking = new Booking(1, user.UserID, kapal1.KapalID, DateTime.Now.AddDays(7),
                                      4, "Bali", user, kapal1);
            booking.BuatPesanan(user.UserID, kapal1.KapalID, DateTime.Now.AddDays(7), 4, "Bali");

            var payment = new Payment(1, booking.BookingID, PaymentMethod.BankTransfer, booking.HitungTotalHarga(), booking);
            payment.ProsesPembayaran(PaymentMethod.BankTransfer, booking.HitungTotalHarga());

            MessageBox.Show(payment.CetakStrukPembayaran(), "Struk Pembayaran");*/
        }
    }
}
