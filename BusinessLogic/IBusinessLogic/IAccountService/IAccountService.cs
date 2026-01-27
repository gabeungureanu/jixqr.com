using DataTypes.ModelDataTypes.Account;
using DataTypes.ModelDataTypes.Common;
using DataTypes.ModelDataTypes.Home;
using DataTypes.ModelDataTypes.OpenAiAPI;
using DataTypes.ModelDataTypes.SMTPSetting;
using DataTypes.ModelDataTypes.Subscription;

namespace BusinessLogic.IBusinessLogic.IAccountService
{
    public interface IAccountService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userInformation"></param>
        /// <returns></returns>
        //Task<Message> InsertUserInformation(UserInformation userInformation);
        //Task<UserInformation> GetUserInformation(string EmailAddress, string Password);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        //Task<UserInformation> GetUserDetails_ByUSERID(Guid? UserId);
        //Task<UserInformation> GetUserDetails_BySubscriptionID(string SubscriptionID);
        //UserInformation verify_UserProfile(Guid UserId, Guid TokenID);
        //Task<UserInformation> GetUserID_ByEmailID(string EmailAddress);
        //List<Testimonies> GetTestimonies(string DomainName);
        //UserInformation Fetch_Registred_Email(string EmailAddress,string DomainName);
        //Int32 Insert_Token(string UserID, string TokenID);
        //UserInformation Check_Token(Guid TokenID);
        //UserInformation UpdatePassword(string NewPassword, string TokenID);
        //UserInformation ChangePassword(Guid UserID, string OldPassword, string NewPassword);
        //Message InsertLoginHistory(string UserID, string AccountUserID, string IPAddress,string SessionID);
        //Message UpdateLoginHistory(string UserID, string IPAddress);
        //string GetInvestorCode(string DomainName);

        //Message UpdateSubscriptionStatusForUser(Guid UserID, string SubscriptionStatus);

        //UserInformation DeleteUserProfile(Guid? UserID, Guid? accountId);
        //Message UpdateSubscriptionStatus(Guid UserID, string SubscriptionStatus, string SubscriptionID);
        #region Extract User Information
        //List<object> GetUserMemory(Guid AccountUserID, Guid UserID, string DomainName);       
        //string AddUserMemory(Guid AccountUserID,Guid userID, string memory, string domain);
        ////List<UserMemoryDomain> GetUserMemoryData(Guid? AccountUserID, Guid? userID);
        //UserMemory DeleteUserMemoryData(Guid? memoryID);
        //UserMemory UpdateMemoryData(Guid MemoryID, string Memorydata);
        //Task<Message> UpdateUserInformation(UserInformation userInformation);
        List<UserInformation> Get_Denomination();
        List<UserInformation> Get_Language(Guid? UserId);
        List<UserInformation> Get_TimeZone();
        List<UserInformation> Get_DateFormat();
        //List<UserInformation> ShowManageUserProfile(Guid? UserId);
        //Task<Message> InsertNewUserInformation(UserInformation userInformation);
        //UserInformation UploadProfileImage(Guid? UserID, UserInformation userInformation);
        #endregion

        //Task<ResponseFeedback> InsertFeedBack(ResponseFeedback objmodel);
        //Task<ReportFeedback> InsertReportFeedback(ReportFeedback objmodel);

        //Task<int> InsertProduct();
        Task<int> Send_OTPToUser(StripePaymentDetails paymentDetails, _SMTPSetting objSMTP, string DomainName, string TemplatePath);
    }
}
