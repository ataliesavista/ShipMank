using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for BeliTiket.xaml
    /// </summary>
    public partial class BeliTiket : Page
    {
        public BeliTiket()
        {
            InitializeComponent();

            LoadInitialData();
        }

        private void LoadInitialData()
        {

        }

        private void LocationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // isi terserah kamu dulu, mungkin masih kosong
        }

    }
}