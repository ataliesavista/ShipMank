using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipMank_WPF.Models
{
    public class ShipType
    {
        public int TypeID { get; private set; }
        public ShipTypeEnum TypeName { get; private set; }
        public string Description { get; set; }

        public ShipType(int typeID, ShipTypeEnum typeName, string description)
        {
            TypeID = typeID;
            TypeName = typeName;
            Description = description;
        }

        public override string ToString()
        {
            return $"{TypeName}: {Description}";
        }
    }
}
