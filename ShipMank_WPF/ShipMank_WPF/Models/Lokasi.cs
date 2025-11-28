using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMank_WPF.Models
{
    public class Lokasi
    {
        public int PortID { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }

        public Lokasi(int portID, string address, string city, string province)
        {
            PortID = portID;
            Address = address;
            City = city;
            Province = province;
        }

        public override string ToString()
        {
            return $"{City}, {Province}";
        }
    }
}
