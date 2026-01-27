using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.OpenAiAPI
{
    public class ChatGPTRequest
    {
        public string Prompt { get; set; } = null!;
        public string Instructions { get; set; } = null!;
    }
}
