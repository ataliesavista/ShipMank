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
    /// Interaction logic for DetailKapal.xaml
    /// </summary>
    public partial class DetailKapal : Page
    {
        public string KapalStatus { get; set; }

        public DetailKapal()
        {
            InitializeComponent();

            // CONTOH 1: Set Status Unavailable (Pesan akan MUNCUL)
            KapalStatus = "Unavailable";

            // CONTOH 2: Set Status Available (Pesan akan HILANG)
            // KapalStatus = "Available";

            // PENTING: Set DataContext ke diri sendiri agar Binding di XAML jalan
            this.DataContext = this;
        }
    }
}
