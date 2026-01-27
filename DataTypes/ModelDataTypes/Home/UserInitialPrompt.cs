using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Home
{
    public class UserInitialPrompt
    {
       public int ConsumedToken { get; set; }
        public string? Response { get; set; }
    }
    public class SessionMessage
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
        public string? Type { get; set; } // optional
    }

}
