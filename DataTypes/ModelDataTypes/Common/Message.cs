using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Common
{
    public class Message
    {
        public string Id { get; set; } = null!;
        public string Msg { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Focus { get; set; } = null!;
        public string AdditionalValue { get; set; } = null!;
        public Int64 TotalRemainingToken { get; set; }
        public Guid? ID { get; set; }
        public bool IsInsufficientTokenBalance { get; set; }
        public Int64 TokenBalancePercentage { get; set; }
        public Int64 TotalPromptToken { get; set; }
    }
}
