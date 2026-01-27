using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Account
{
    public class RememberMe
    {
        public string EmailAddress { get; set; } = null!;
        public string Password { get; set; }=null!;
    }
}
