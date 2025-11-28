using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMank_WPF.Models.ViewModel
{
    public class ShipViewModel
    {
        public int KapalID { get; set; }
        public string ShipName { get; set; }
        public string ShipClass { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string FullLocation => $"{Address}, {City}, {Province}";
        public string Capacity { get; set; }
        public string Rating { get; set; }
        public string Price { get; set; }
        public string PriceUnit { get; set; } = "/day";
        public string KapalStatus { get; set; }
        public List<string> Facilities { get; set; } = new List<string>();
        public string ImageSource { get; set; }
        public string BadgeColor { get; set; }

        public static string GetBadgeColor(string shipType)
        {
            string type = shipType.ToLower();
            if (type.Contains("phinisi")) return "#8E44AD";
            else if (type.Contains("speedboat")) return "#2980B9";
            else if (type.Contains("yacht")) return "#F39C12";
            else if (type.Contains("ferry")) return "#27AE60";
            else return "#16A085";
        }
    }
}
