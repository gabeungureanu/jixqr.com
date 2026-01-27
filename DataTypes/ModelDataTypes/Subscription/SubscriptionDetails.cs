using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public class SubscriptionDetails
    {
        public string EmailAddress { get; set; } = null!;
        public string CardNumber { get; set; } = null!;
        public string ExpiryMonthYear { get; set; } = null!;
        public int CVV { get; set; }
        public string NameOnCard { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string ZIPCode { get; set; } = null!;
    }
}
