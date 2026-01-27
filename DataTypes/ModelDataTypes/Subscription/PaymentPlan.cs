using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public class PaymentPlan
    {
        public Guid? PaymentPlanID { get; set; }
        public string PlanName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Decimal PlanPrice { get; set; }
        public bool FeaturedPlan { get; set; }
        public string PlanType { get; set; } = null!;
        public Guid? PlanTypeID { get; set; } = null!;
        public Guid? CurrentPlanID { get; set; }
        public string PlanStatus { get; set; } = null!;
        public int PlanIndex { get; set; }
        public List<PaymentPlanItem>? PlanItems { get; set; }
    }
}
