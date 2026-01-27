using BusinessLogic.IBusinessLogic.SMTP_Setting;
using Dapper;
using DataAccess;
using DataTypes.ModelDataTypes.SMTPSetting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.BLImplementation.SMTP_Setting
{
    public class SMTP_SettingService : ISMTP_SettingService
    {
        #region Singletone Method
        private static SMTP_SettingService _current;

        public static SMTP_SettingService Current()
        {
            if (_current == null)
            {
                _current = new SMTP_SettingService();
            }
            return _current;
        }
        #endregion

        #region Fetch Server Settings By SMTPName

        /// <summary>
        /// To fetch SMTP settings from database
        /// </summary>
        /// <param name="SMTPName"></param>
        /// <returns></returns>
        public _SMTPSetting Fetch_SMTP_Settings_By_SMTPName(string SMTPName)
        {
            try
            {
                _SMTPSetting obj = new _SMTPSetting();
                DynamicParameters objparameter = new DynamicParameters();
                //objparameter.Add("SMTPName", "uat.askjubileegpt.com");
                objparameter.Add("SMTPName", "jubileechat.com");
                obj = FactoryServices.dbFactory.SelectCommand_SP(obj, "GET_Email_Server_Settings", objparameter);
                return obj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
