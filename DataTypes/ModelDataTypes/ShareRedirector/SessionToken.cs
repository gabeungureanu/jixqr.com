using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.ShareRedirector
{
    public class SessionToken
    {
        public string? Key { get; set; }
        public string? Value { get; set; }
    }
    public class CookieValue
    {
        public string? ShareId { get; set; }
        public string? Cookie { get; set; }
    }
}
