using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.SMTPSetting
{
    public class _SMTPSetting
    {
        public int AccountId { get; set; }
        public int EmailId { get; set; }
        public int SMTPId { get; set; }
        public string? SMTPName { get; set; }
        public string? SMTPServer { get; set; }
        public string? SenderEmail { get; set; }
        public string? SenderPassword { get; set; }
        public string? DisplayName { get; set; }
        public int SMTPPort { get; set; }
        public int DisplayOrder { get; set; }
        public bool SSlEnable { get; set; }
        public int objSMTP { get; set; }
    }
}
