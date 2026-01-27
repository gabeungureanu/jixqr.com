using DataTypes.ModelDataTypes.SMTPSetting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.IBusinessLogic.SMTP_Setting
{
    public interface ISMTP_SettingService
    {
        _SMTPSetting Fetch_SMTP_Settings_By_SMTPName(string SMTPName);
    }
}
