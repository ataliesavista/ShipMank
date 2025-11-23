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
    /// Interaction logic for Payment.xaml
    /// </summary>
    public partial class Payment : Page
    {
        public Payment()
        {
            InitializeComponent();

            TxtNamaKapal.Text = "Kapal Babe Asep";
            TxtShipType.Text = "Luxury Speedboat";
            TxtDateBerangkat.Text = DateTime.Now.AddDays(2).ToString("dddd, dd MMM yyyy");
            TxtLokasi.Text = "Marina Ancol -> Pulau Pramuka";
            TxtKapasitas.Text = "12 Orang";
            TxtHargaPerjalanan.Text = "Rp 2.000.000";
            TxtTotal.Text = "Rp 2.005.000";
        }
    }
}
