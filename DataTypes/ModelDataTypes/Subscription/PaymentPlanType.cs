using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public class PaymentPlanType
    {
        public Guid PlanTypeID { get; set; }
        public string PlanName { get; set; } = null!;
        public List<PaymentPlan> PaymentPlan { get; set; }
        public List<PaymentPlanItem> PaymentPlanItem { get; set; }
    }
}
