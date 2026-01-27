using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Common
{
    public class Language
    {
        public Guid LanguageID { get; set; }
        public string LanguageName { get; set; } = null!;
        public string LanguageFlag { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Prompt { get; set; } = null!;
        public bool IsDefault{ get; set; }
        public bool IsFirstResponseUpend { get; set; }
        public int UsageCount { get; set; }
    }
}
