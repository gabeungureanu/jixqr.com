using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.OpenAiAPI
{
    public class ExtractUserInfo
    {
        public Guid UserID { get; set; }
        public string? UserName { get; set; }
        public string? Location { get; set; }
        public string? Profession { get; set; }
        public string[]? Skills { get; set; }
        public string[]? Projects { get; set; }
        public string[]? Hobbies { get; set; }
        public string[]? Education { get; set; }
        public string[]? ContactInfo { get; set; }
        public string[]? Preferences { get; set; }
        public string[]? SocialMediaLinks { get; set; }
        public string[]? Status { get; set; }
        public string[]? OtherPersonalDetails { get; set; }
    }

    public class UserInfo
    {
        public string? MemoryData { get; set; }
        
    }
}
