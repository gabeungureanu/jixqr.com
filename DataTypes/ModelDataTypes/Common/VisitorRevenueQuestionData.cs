using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Common
{
    public class VisitorRevenueQuestionData
    {
        public int DomainTotalVisitors { get; set; }
        public int TotalVisitors { get; set; }
        public int DomainTotalQuestions { get; set; }
        public int TotalQuestions { get; set; }
        public decimal DomainTotalRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<RevenueGraphData> revenueGraphDatas { get; set; }
    }

    public class RevenueGraphData
    {
        public decimal Revenue { get; set; }
        public string? RevenueDate { get; set; }
    }
}
