using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public class OpenAIBalance
    {
        public double TotalCredits { get; set; }
        public double UsedCredits { get; set; }
        public double RemainingCredits { get; set; }
    }

}
