using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public class PaymentPlanItem
    {
        public Guid? PaymentPlanItemID { get; set; }
        public string Item { get; set; } = null!;

        public Guid FeatureID { get; set; }
        public Guid PlanTypeID { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? plan1 { get; set; }
        public string? plan2 { get; set; }
        public string? plan3 { get; set; }
        public string? plan4 { get; set; }
        public string? plan5 { get; set; }
        public string? plan6 { get; set; }
    }
}
