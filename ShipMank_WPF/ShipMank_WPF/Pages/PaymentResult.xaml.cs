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
    /// Interaction logic for PaymentResult.xaml
    /// </summary>
    public partial class PaymentResult : Window
    {
        public PaymentResult(string bank, string vaNumber, string amount)
        {
            InitializeComponent();
            TxtBank.Text = $"VIRTUAL ACCOUNT {bank.ToUpper()}";
            TxtVA.Text = vaNumber;
            TxtAmount.Text = amount;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

}
