using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public class SubscriptionPlanInfo
    {
        public Guid PaymentPlanID { get; set; }
        public string PlanName { get; set; } = null!;
        public string PlanType { get; set; } = null!;
        public decimal PlanPrice { get; set; } 
        public string Description { get; set; } = null!;
        public string SubscriptionID { get; set; } = null!;
        public string InvoiceID { get; set; } = null!;
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public string? Status { get; set; }
    }
}
