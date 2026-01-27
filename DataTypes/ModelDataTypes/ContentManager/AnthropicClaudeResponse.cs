using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.ContentManager
{
    public class AnthropicClaudeResponse
    {
        public string sessionID { get; set; } = null!;
        public string response { get; set; } = null!;
        public int inputToken { get; set; }
        public int outputToken { get; set; }
    }
}
