using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Home
{
    public class ModePrompt
    {
        public Guid? ParentID { get; set; }
        public string? ParentCategory { get; set; }
        public string? ParentIcon { get; set; }
        public List<ModePromptChild> modePromptChildren { get; set; }
    }

    public class ModePromptChild
    {
        public Guid? ChildID { get; set; }
        public string? ChildCategory { get; set; }
        public string? SubPrompt { get; set; }
        public string? ChildIcon { get; set; }
        public string? ParentIcon { get; set; }
    }
}
