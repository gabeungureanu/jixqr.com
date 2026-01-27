using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Home
{

    public class UserMemoryDomain
    {       
        public string? Domain { get; set; }
        public int id { get; set; }        
        public List<UserMemory> MemoryList { get; set; }

        public UserMemoryDomain()
        {
            MemoryList = new List<UserMemory>();
        }
    }


    public class UserMemory
    {
        public Guid MemoryID { get; set; }
        public string? Domain { get; set; }
        public Guid UserID { get; set; }
        public string? MemoryData { get; set; }
        public string? Type { get; set; }
        public string? DisplayOrder { get; set; }
        public string? Msg { get; set; }
    }
}
