using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Home
{
    public class PromptResponse
    {
        public Guid? ResponseID { get; set; }
        public string Prompt { get; set; } = null!;
        public string Response { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
    }
}
