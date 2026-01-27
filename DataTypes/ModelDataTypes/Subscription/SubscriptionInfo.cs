using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public class SubscriptionInfo
    {
        public int SubID { get; set; }
        public Guid userId { get; set; }
        public string subscriptionID { get; set; }
        public string sessionID { get; set; }
        public string productID { get; set; }
        public string priceID { get; set; }
        public string Status { get; set; }
    }
}
