using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Home
{
    public class PromptResponseReturn
    {
        public Guid? ParentID { get; set; }
        public Guid? ID { get; set; }
        public string? Response { get; set; }
        public string? ApiResponse { get; set; }
        public string? LoginStatus { get; set; }
        public string? Prompt { get; set; }
        public string? DomainName { get; set; }
        public string? IsMaxRequestReached { get; set; }
        public bool IsInsufficientTokenBalance { get; set; }
        public string? ShareLink { get; set; }
        public Guid? ShareChatID { get; set; }
        public Guid? PromptID { get; set; }
        public string? Msg { get; set; }
        public string? FeedBackType { get; set; }
        public Int64 TotalRemainingToken { get; set; }
        public Int64 TokenBalancePercentage { get; set; }
    }
}
