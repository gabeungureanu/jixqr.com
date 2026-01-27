using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.WebsiteSettings
{
    public class WebsiteConfiguration
    {
        public Guid? System_WebsiteID { get; set; }
        public string System_WebsiteName { get; set; } = null!;
        public string System_DomainName { get; set; } = null!;
        public string System_WebsitePrefix { get; set; } = null!;
        public string System_WebsiteMiddle { get; set; } = null!;
        public string System_WebsiteSuffix { get; set; } = null!;
        public string ShortDisclaimer { get; set; } = null!;
        public string Disclaimer { get; set; } = null!;
        public string PromptDisclaimer { get; set; } = null!;
        public string TitleText { get; set; } = null!;
        public string FaviconImagePath { get; set; } = null!;
        public string MainImagePath { get; set; } = null!;
        public string BrandImagePath { get; set; } = null!;
        public string MainImageAltText { get; set; } = null!;
        public string ResReplacedText { get; set; } = null!;
        public string PlanName { get; set; } = null!;
    }
}
