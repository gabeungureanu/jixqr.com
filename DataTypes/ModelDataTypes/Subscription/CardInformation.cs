using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public class CardInformation
    {
        public string? UserId { get; set; }
        public string? CardBrand { get; set; }
        public string? Last4Digits { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
    }
}
