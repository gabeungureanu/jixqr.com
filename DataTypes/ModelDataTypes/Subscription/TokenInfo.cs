using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public class TokenInfo
    {
        public Int64 TotalRequestToken { get; set; }
        public Int64 TotalResponseToken { get; set; }
        public Int64 TotalAvailableToken { get; set; }
        public Int64 TotalRemainingToken { get; set; }
    }
}
