using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Account
{
    public class UserSubscription
    {
        public Guid UserID { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string CardNumber { get; set; } = null!;
        public int ExpireMonth { get; set; }
        public int ExpireYear { get; set; }
        public string CVC { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public string EmailAddress { get; set; } = null!;
        public string CustomerName { get; set; } = null!;

    }
}
