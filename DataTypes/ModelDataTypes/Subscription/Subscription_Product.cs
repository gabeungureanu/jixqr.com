using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public class Subscription_Product
    {
        public int ProductId { get; set; }
        public string Stripe_ProductID { get; set; } = null!;
    }
}
