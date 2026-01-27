using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public class Features
    {
        public int FeatureID { get; set; }
        public string FeatureName { get; set; }
        public bool BasicFree { get; set; }
        public bool Standard { get; set; }
        public bool Deluxe { get; set; }
        public bool Professional { get; set; }
        public bool EnterPrise { get; set; }

    }
}
