using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public class PaymentMethodDetail
    {
        public string? PaymentMethodID { get; set; }
        public string? Type { get; set; }
        public string? CardType { get; set; }
        public string? CardLastFourDigits { get; set; }
        public string? ExpireMonth { get; set; }
        public string? ExpireYear { get; set; }
        public string? Country { get; set; }
        public string? LoginStatus { get; set; }
        public string? DomainName { get; set; }
        public bool IsDefault { get; set; }
    }
}
