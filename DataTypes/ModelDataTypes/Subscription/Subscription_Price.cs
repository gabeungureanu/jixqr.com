using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public class Subscription_Price
    {
        public int ProductId { get; set; }
        public string Striper_PriceId { get; set; } = null!;
    }
}
