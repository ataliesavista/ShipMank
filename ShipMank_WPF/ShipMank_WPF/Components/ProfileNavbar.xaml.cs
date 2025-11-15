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
    /// Interaction logic for ProfileNavbar.xaml
    /// </summary>
    public partial class ProfileNavbar : UserControl
    {
        public event RoutedEventHandler NavigatePassengerList;
        public event RoutedEventHandler NavigateMyAccount;

        public ProfileNavbar()
        {
            InitializeComponent();

            PassengerListButton.Click += (s, e) => NavigatePassengerList?.Invoke(this, e);
            MyAccountButton.Click += (s, e) => NavigateMyAccount?.Invoke(this, e);
        }
    }
}
