using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Home
{
    public class UserParentNodePrompt
    {
        public Guid? PromptFolderID { get; set; }
        public string PromptText { get; set; } = null!;
        public bool IsFavourite { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<UserPrompt> userPrompts { get; set; }
    }
}
