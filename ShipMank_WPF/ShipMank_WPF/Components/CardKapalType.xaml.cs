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
    /// Interaction logic for CardKapalType.xaml
    /// </summary>
    public partial class CardKapalType : UserControl
    {
        public CardKapalType()
        {
            InitializeComponent();
        }

        // Image Source (gambar)
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(CardKapalType), new PropertyMetadata(null));


        // Judul Kapal
        public string JudulKapal
        {
            get { return (string)GetValue(JudulKapalProperty); }
            set { SetValue(JudulKapalProperty, value); }
        }

        public static readonly DependencyProperty JudulKapalProperty =
            DependencyProperty.Register("JudulKapal", typeof(string), typeof(CardKapalType), new PropertyMetadata("Judul Kapal"));


        // Deskripsi Kapal
        public string DeskripsiKapal
        {
            get { return (string)GetValue(DeskripsiKapalProperty); }
            set { SetValue(DeskripsiKapalProperty, value); }
        }

        public static readonly DependencyProperty DeskripsiKapalProperty =
            DependencyProperty.Register("DeskripsiKapal", typeof(string), typeof(CardKapalType), new PropertyMetadata("Deskripsi kapal disini..."));

    }
}
