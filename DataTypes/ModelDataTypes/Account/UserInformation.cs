using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Account
{
    public class UserInformation
    {
        public Guid UserID { get; set; }
        public Guid AccountUserID { get; set; }
        public string? FirstName { get; set; } = null!;
        public string? LastName { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? EmailAddress { get; set; } = null!;
        public string? Password { get; set; } = null!;
        public string? DiscountCode { get; set; } = null!;
        public bool IsDiscountCode { get; set; }
        public bool IsLocked { get; set; }

        public bool IsAdmin { get; set; }
        public bool IsShowLanguage { get; set; }
        public string? AccountStatus { get; set; } = null!;
        public bool IsEmailVerified { get; set; }
        public string? Status { get; set; } = "0";
        public string Message { get; set; } = null!;
        public string Focus { get; set; } = null!;
        public string? Msg { get; set; } = null!;
        public bool IsTermCondition { get; set; }
        public string? ProfileImage { get; set; } = null!;
        public string? ProfileImagePath { get; set; } = null!;
        public string? ProfileImageClass { get; set; } = null!;
        public string? Occupation { get; set; } = null!;
        public bool IsProfileImageClass { get; set; }

        /****Denomination*******/
        public int Denomination { get; set; }
        public string? DenominationName { get; set; } = null!;

        /****Langauages*******/
        public Guid LanguageID { get; set; }
        public string? LanguageName { get; set; } = null!;

        /****Timezone*******/
        public long Timezone { get; set; }
        public string? TimezoneName { get; set; } = null!;

        /****Date Format*******/
        public int DateFormat { get; set; }
        public string? DateFormatDescription { get; set; } = null!;
        public bool IsEnable { get; set; } 
    }
}
