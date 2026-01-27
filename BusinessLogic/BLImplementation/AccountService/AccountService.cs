using BusinessLogic.IBusinessLogic.IAccountService;
using BusinessLogic.IBusinessLogic.ICryptoService;
using BusinessLogic.IBusinessLogic.IWebsiteSettingsService;
using Dapper;
using DataAccess;
using DataTypes.ModelDataTypes.Account;
using DataTypes.ModelDataTypes.Common;
using DataTypes.ModelDataTypes.Home;
using DataTypes.ModelDataTypes.OpenAiAPI;
using DataTypes.ModelDataTypes.SMTPSetting;
using DataTypes.ModelDataTypes.Subscription;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using OpenAI_API.Moderation;
using SharpToken;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace BusinessLogic.BLImplementation.AccountService
{
    public class AccountService : IAccountService
    {
        private readonly ICryptographyService _cryptographyService;
        private DbFactory _dBFactory;
        private IWebsiteSettings _websiteSettings;

        public AccountService(ICryptographyService cryptographyService, DbFactory dbFactory, IWebsiteSettings websiteSettings)
        {
            _cryptographyService = cryptographyService;
            _websiteSettings = websiteSettings;
            _dBFactory = dbFactory;
        }

        /// <summary>
        /// Service to save user information into database
        /// </summary>
        /// <param name="userInformation"></param>
        /// <returns></returns>
        //public async Task<Message> InsertUserInformation(UserInformation userInformation)
        //{
        //    try
        //    {
        //        string DomainName = _websiteSettings.GetDomain();
        //        Message message = new Message();
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@FirstName", userInformation.FirstName);
        //        dParam.Add("@LastName", userInformation.LastName);
        //        dParam.Add("@UserName", userInformation.UserName);
        //        dParam.Add("@EmailAddress", userInformation.EmailAddress);
        //        dParam.Add("@Password", _cryptographyService.Encrypt(userInformation.Password!.Trim()));
        //        dParam.Add("@IsDiscountCode", userInformation.IsDiscountCode);
        //        dParam.Add("@DiscountCode", userInformation.DiscountCode);
        //        dParam.Add("@DomainName", DomainName);
        //        message = await _dBFactory.InsertCommand_SPQueryAsync<Message>(message, "system_Users_Create", dParam);
        //        return message;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public List<Testimonies> GetTestimonies(string DomainName)
        //{
        //    try
        //    {
        //        List<Testimonies> testimonies = new List<Testimonies>();

        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@DomainName", DomainName);
        //        testimonies = _dBFactory.SelectCommand_SP<Testimonies>(testimonies, "website_Testimonies_Get", dParam);
        //        return testimonies;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        /// <summary>
        /// Get user information
        /// </summary>
        /// <param name="EmailAddress"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        //public async Task<UserInformation> GetUserInformation(string EmailAddress, string Password)
        //{
        //    string DomainName = _websiteSettings.GetDomain();
        //    UserInformation userInformation = new UserInformation();
        //    DynamicParameters dParam = new DynamicParameters();
        //    dParam.Add("@EmailAddress", EmailAddress);
        //    dParam.Add("@Password", _cryptographyService.Encrypt(Password));
        //    dParam.Add("@DomainName", DomainName);
        //    userInformation = await _dBFactory.SelectCommand_SPAsync(userInformation, "system_Users_Get", dParam);
        //    return userInformation;
        //}

        #region
        // Code to Get UserID by EmailID
        //public async Task<UserInformation> GetUserID_ByEmailID(string EmailAddress)
        //{
        //    try
        //    {
        //        string domainName = _websiteSettings.GetDomain();
        //        UserInformation objDetails = new UserInformation();
        //        DynamicParameters objparameter = new DynamicParameters();
        //        objparameter.Add("EmailAddress", EmailAddress);
        //        objparameter.Add("DomainName", domainName);
        //        objDetails = FactoryServices.dbFactory.SelectCommand_SP(objDetails, "Get_Userid_by_EmailID", objparameter);
        //        return objDetails;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        #endregion

        #region
        //public UserInformation verify_UserProfile(Guid UserId, Guid TokenID)
        //{
        //    try
        //    {
        //        UserInformation objverify = new UserInformation();
        //        string status = "";

        //        DynamicParameters objparameter = new DynamicParameters();
        //        objparameter.Add("UserId", Convert.ToString(UserId)); // No need to parse as Guid here
        //        objverify = _dBFactory.SelectCommand_SP(objverify, "Verify_UserDetails", objparameter);
        //        return objverify;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        #endregion

        #region

        /// <summary>
        /// To get user details by ID
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        //public async Task<UserInformation> GetUserDetails_ByUSERID(Guid? UserId)
        //{
        //    try
        //    {
        //        UserInformation objDetails = new UserInformation();
        //        DynamicParameters objparameter = new DynamicParameters();
        //        objparameter.Add("UserId", UserId);
        //        objDetails = FactoryServices.dbFactory.SelectCommand_SP(objDetails, "Get_User_details_for_Verify_Account_Mail", objparameter);
        //        return objDetails;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public async Task<UserInformation> GetUserDetails_BySubscriptionID(string SubscriptionID)
        //{
        //    try
        //    {
        //        UserInformation objDetails = new UserInformation();
        //        DynamicParameters objparameter = new DynamicParameters();
        //        objparameter.Add("SubscriptionID", SubscriptionID);
        //        objDetails = FactoryServices.dbFactory.SelectCommand_SP(objDetails, "Get_User_details_SubscriptionID", objparameter);
        //        return objDetails;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        #endregion

        //----------------------------------updated code---------------------------------------

        //public UserInformation Fetch_Registred_Email(string EmailAddress, string DomainName)
        //{
        //    try
        //    {
        //        int retval = 0;
        //        UserInformation userInformation = new UserInformation();
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@EmailAddress", EmailAddress);
        //        dParam.Add("@DomainName", DomainName);
        //        userInformation = _dBFactory.SelectCommand_SP(userInformation, "Fetch_Registred_Email", dParam);

        //        return userInformation;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public Int32 Insert_Token(string UserID, string TokenID)
        //{
        //    Int32 retval = 0;
        //    try
        //    {
        //        UserInformation userInformation = new UserInformation();
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@UserID", UserID);
        //        dParam.Add("@Token", TokenID);
        //        userInformation = _dBFactory.SelectCommand_SP(userInformation, "Insert_ForgotPassword_Token", dParam);
        //        return retval;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public Message UpdateSubscriptionStatusForUser(Guid UserID, string SubscriptionStatus)
        //{
        //    try
        //    {
        //        Message message = new Message();
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@UserID", UserID);
        //        dParam.Add("@SubscriptionStatus", SubscriptionStatus);
        //        message = _dBFactory.SelectCommand_SP(message, "Subscription_SubStatusForUser_Update", dParam);
        //        return message;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        //public Message UpdateSubscriptionStatus(Guid UserID, string SubscriptionStatus, string SubscriptionID)
        //{
        //    try
        //    {
        //        Message message = new Message();
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@UserID", UserID);
        //        dParam.Add("@SubscriptionStatus", SubscriptionStatus);
        //        dParam.Add("@SubscriptionID", SubscriptionID);
        //        message = _dBFactory.SelectCommand_SP(message, "subscription_SubStatus_Update", dParam);
        //        return message;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        //public UserInformation Check_Token(Guid TokenID)
        //{
        //    //string retval = "";
        //    try
        //    {
        //        UserInformation userInformation = new UserInformation();
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@Token", TokenID);
        //        userInformation = _dBFactory.SelectCommand_SP(userInformation, "Validate_Token", dParam);
        //        return userInformation;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        ////public UserInformation UpdatePassword(string NewPassword, string TokenID)
        ////{
        ////    try
        ////    {
        ////        //int retval = 0;
        ////        UserInformation userInformation = new UserInformation();
        ////        DynamicParameters dParam = new DynamicParameters();
        ////        dParam.Add("@Token", TokenID);
        ////        dParam.Add("@Password", _cryptographyService.Encrypt(NewPassword));
        ////        userInformation = _dBFactory.InsertCommand_SPQuery(userInformation, "System_Users_ResetPassword", dParam);
        ////        return userInformation;
        ////    }
        ////    catch (Exception)
        ////    {
        ////        throw;
        ////    }
        ////}

        //public UserInformation ChangePassword(Guid UserID, string OldPassword, string NewPassword)
        //{
        //    try
        //    {
        //        //int retval = 0;
        //        UserInformation userInformation = new UserInformation();
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@UserID", UserID);
        //        dParam.Add("@OldPassword", _cryptographyService.Encrypt(OldPassword));
        //        dParam.Add("@NewPassword", _cryptographyService.Encrypt(NewPassword));
        //        userInformation = _dBFactory.InsertCommand_SPQuery(userInformation, "System_Users_ChangePassword", dParam);
        //        return userInformation;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public Message InsertLoginHistory(string UserID, string AccountUserID, string IPAddress, string SessionID)
        //{
        //    try
        //    {
        //        Message message = new Message();
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@UserID", UserID);
        //        dParam.Add("@AccountUserID", AccountUserID);
        //        dParam.Add("@IPAddress", IPAddress);
        //        dParam.Add("@SessionID", SessionID);
        //        message = _dBFactory.InsertCommand_SPQuery<Message>(message, "system_LoginHistory_Add", dParam);
        //        return message;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public Message UpdateLoginHistory(string UserID, string IPAddress)
        //{
        //    try
        //    {
        //        Message message = new Message();
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@UserID", UserID);
        //        dParam.Add("@IPAddress", IPAddress);
        //        message = _dBFactory.InsertCommand_SPQuery<Message>(message, "system_LoginHistory_Update", dParam);
        //        return message;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public string GetInvestorCode(string DomainName)
        //{
        //    try
        //    {
        //        string inverstorCode = string.Empty;
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@DomainName", DomainName);
        //        inverstorCode = _dBFactory.SelectCommand_SP<string>(inverstorCode, "system_InvestorDiscount_Get", dParam);
        //        return inverstorCode;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        #region Extract User Information
        //Get User Memory Information
        //public List<object> GetUserMemory(Guid AccountUserID, Guid UserID, string DomainName)
        //{
        //    List<object> userMemoryList = new List<object>();
        //    try
        //    {
        //        // Initialize the GPT-3 tokenizer
        //        var tokenizer = GptEncoding.GetEncoding("cl100k_base");

        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@UserID", UserID);
        //        dParam.Add("@AccountUserID", AccountUserID);
        //        dParam.Add("@DomainName", DomainName);
        //        var LstExtractUserInfo = _dBFactory.SelectCommand_SP<UserInfo>("user_Memory_Get", dParam);
        //        if (LstExtractUserInfo != null && LstExtractUserInfo.Count > 0)
        //        {
        //            int totalToken = 0;
        //            // Convert each record into an object and add it to the list
        //            foreach (var record in LstExtractUserInfo)
        //            {
        //                // Tokenize the input
        //                var tokens = tokenizer.Encode(record.MemoryData);
        //                totalToken += tokens.Count;
        //                if (totalToken < 32000)
        //                {
        //                    userMemoryList.Add(new { role = "system", content = record.MemoryData, type = "memory" });
        //                }
        //            }
        //        }
        //        return userMemoryList;
        //    }
        //    catch (Exception)
        //    {
        //        return new List<object>();
        //    }
        //}
        /// <summary>
        /// Memory Add
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="memory"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        //public string AddUserMemory(Guid AccountUserID, Guid userID, string memory, string domain)
        //{
        //    string result = string.Empty;
        //    try
        //    {
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@AccountUserID", AccountUserID);
        //        dParam.Add("@UserID", userID);
        //        dParam.Add("@Memory", memory);
        //        dParam.Add("@Domain", domain);
        //        return _dBFactory.SelectCommand_SP<string>(result, "user_Memory_Insert", dParam);
        //    }
        //    catch (Exception)
        //    {
        //        return "false";
        //    }
        //}

        /// <summary>
        /// Retrieve the user's stored memory data.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        //public List<UserMemoryDomain> GetUserMemoryData(Guid? AccountUserID, Guid? userID)
        //{
        //    List<UserMemoryDomain> userMemoryList = new List<UserMemoryDomain>();
        //    List<UserMemory> MemoryList = new List<UserMemory>();
        //    try
        //    {
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@AccountUserID", AccountUserID);
        //        dParam.Add("@UserID", userID);
        //        MemoryList = _dBFactory.SelectCommand_SP(MemoryList, "user_Memory_List_Get", dParam);
        //        if (MemoryList != null && MemoryList.Count > 0)
        //        {
        //            userMemoryList = MemoryList
        //                             .GroupBy(x => x.Domain) // Group by the domain property
        //                             .Select(g => new UserMemoryDomain { Domain = g.Key })
        //                             .ToList();
        //            if (userMemoryList != null && userMemoryList.Count > 0)
        //            {
        //                int i = 1;
        //                foreach (var item in userMemoryList)
        //                {
        //                    item.id = i;
        //                    item.MemoryList = MemoryList.Where(x => x.Domain == item.Domain).ToList();
        //                    i++;
        //                }
        //            }

        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return userMemoryList ?? new List<UserMemoryDomain>();
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        //public UserMemory DeleteUserMemoryData(Guid? memoryID)
        //{
        //    UserMemory userParentNodePrompts = new UserMemory();
        //    try
        //    {
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@MemoryID", memoryID);
        //        userParentNodePrompts = _dBFactory.SelectCommand_SP(userParentNodePrompts, "user_Memory_delete", dParam);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return userParentNodePrompts;
        //}

        //public UserMemory UpdateMemoryData(Guid MemoryID, string Memorydata)
        //{
        //    UserMemory userParentNodePrompts = new UserMemory();
        //    try
        //    {
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@MemoryID", MemoryID);
        //        dParam.Add("@Memorydata", Memorydata);
        //        userParentNodePrompts = _dBFactory.SelectCommand_SP(userParentNodePrompts, "user_Memory_Update", dParam);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return userParentNodePrompts;
        //}
        #endregion
        #region Share Redirector Service
        #endregion

        //public async Task<Message> UpdateUserInformation(UserInformation userInformation)
        //{
        //    try
        //    {
        //        string DomainName = _websiteSettings.GetDomain();
        //        string defaultImage = "avatar-juan.png";
        //        string defaultPath = "/Images";
        //        Message message = new Message();
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@UserName", userInformation.UserName);
        //        dParam.Add("@FirstName",userInformation.FirstName);
        //        dParam.Add("@LastName", userInformation.LastName);
        //        dParam.Add("@EmailAddress", userInformation.EmailAddress);
        //        dParam.Add("@Password", _cryptographyService.Encrypt(userInformation.Password!.Trim()));
        //        if (userInformation.IsProfileImageClass == false)
        //        {
        //            dParam.Add("@ProfileImage", string.IsNullOrEmpty(userInformation.ProfileImage) ? defaultImage : userInformation.ProfileImage);
        //            dParam.Add("@ProfileImagePath", string.IsNullOrEmpty(userInformation.ProfileImagePath) ? defaultPath : userInformation.ProfileImagePath);
        //            dParam.Add("@ProfileImageClass", "");
        //        }
        //        else
        //        {
        //            dParam.Add("@ProfileImage", "");
        //            dParam.Add("@ProfileImagePath", "");
        //            dParam.Add("@ProfileImageClass", userInformation.ProfileImageClass);
        //        }
        //        dParam.Add("@IsProfileImageClass", userInformation.IsProfileImageClass);
        //        dParam.Add("@UserID", userInformation.UserID);
        //        dParam.Add("@AccountUserID", userInformation.AccountUserID);
        //        dParam.Add("@Occupation", userInformation.Occupation);
        //        dParam.Add("@Denomination", userInformation.Denomination);
        //        dParam.Add("@DefaultLanguage", userInformation.LanguageID);
        //        dParam.Add("@Timezone", userInformation.Timezone);
        //        dParam.Add("@DateFormat", userInformation.DateFormat);
        //        dParam.Add("@DomainName", DomainName);
        //        dParam.Add("@IsLocked", userInformation.IsLocked);
        //        dParam.Add("@IsEnable", userInformation.IsEnable);
        //        message = await _dBFactory.InsertCommand_SPQueryAsync<Message>(message, "System_User_Update", dParam);
        //        return message;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        public List<UserInformation> Get_Denomination()
        {
            try
            {
                List<UserInformation> objDetails = new List<UserInformation>();
                DynamicParameters dParam = new DynamicParameters();
                objDetails = FactoryServices.dbFactory.SelectCommand_SP(objDetails, "Get_Denominations", dParam);
                return objDetails;
            }
            catch (Exception)
            {
                throw ;
            }
        }
        public List<UserInformation> Get_Language(Guid? UserId)
        {
            try
            {
                List<UserInformation> userInformation = new List<UserInformation>();
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@UserID", UserId);
                userInformation = FactoryServices.dbFactory.SelectCommand_SP(userInformation, "website_Language_Get", dParam);
                return userInformation;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<UserInformation> Get_TimeZone()
        {
            try
            {
                List<UserInformation> userInformation = new List<UserInformation>();
                DynamicParameters dParam = new DynamicParameters();
                userInformation = FactoryServices.dbFactory.SelectCommand_SP(userInformation, "system_TimeZones_Get", dParam);
                return userInformation;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<UserInformation> Get_DateFormat()
        {
            try
            {
                List<UserInformation> userInformation = new List<UserInformation>();
                DynamicParameters dParam = new DynamicParameters();
                userInformation = FactoryServices.dbFactory.SelectCommand_SP(userInformation, "system_DateFormat_Get", dParam);
                return userInformation;
            }
            catch (Exception)
            {
                throw;
            }
            ;
        }
        //public List<UserInformation> ShowManageUserProfile(Guid? UserId)
        //{
        //    try
        //    {
        //        List<UserInformation> userInformation = new List<UserInformation>();
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@AccountUserID", UserId);
        //        userInformation = FactoryServices.dbFactory.SelectCommand_SP(userInformation, "Get_All_user_details", dParam);
        //        return userInformation;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //}
        //public async Task<Message> InsertNewUserInformation(UserInformation userInformation)
        //{
        //    try
        //    {
        //        string DomainName = _websiteSettings.GetDomain();
        //        Message message = new Message();
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@UserName", userInformation.UserName);
        //        dParam.Add("@FirstName", userInformation.FirstName);
        //        dParam.Add("@LastName", userInformation.LastName);
        //        dParam.Add("@EmailAddress", userInformation.EmailAddress);
        //        dParam.Add("@Password", _cryptographyService.Encrypt(userInformation.Password!.Trim()));
        //        if (userInformation.IsProfileImageClass == false)
        //        {
        //            dParam.Add("@ProfileImage", userInformation.ProfileImage);
        //            dParam.Add("@ProfileImagePath", userInformation.ProfileImagePath);
        //            dParam.Add("@ProfileImageClass", "");
        //        }
        //        else
        //        {
        //            dParam.Add("@ProfileImage", "");
        //            dParam.Add("@ProfileImagePath", "");
        //            dParam.Add("@ProfileImageClass", userInformation.ProfileImageClass);
        //        }
        //        dParam.Add("@IsProfileImageClass", userInformation.IsProfileImageClass);
        //        dParam.Add("@AccountUserID", userInformation.AccountUserID);
        //        dParam.Add("@Occupation", userInformation.Occupation);
        //        dParam.Add("@Denomination", userInformation.Denomination);
        //        dParam.Add("@DefaultLanguage", userInformation.LanguageID);
        //        dParam.Add("@Timezone", userInformation.Timezone);
        //        dParam.Add("@DateFormat", userInformation.DateFormat);
        //        dParam.Add("@DomainName", DomainName);
        //        dParam.Add("@IsLocked", userInformation.IsLocked);
        //        dParam.Add("@IsEnable", userInformation.IsEnable);
        //        message = await _dBFactory.InsertCommand_SPQueryAsync<Message>(message, "System_UserProfile_Insert", dParam);
        //        return message;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        //public UserInformation UploadProfileImage(Guid? UserID, UserInformation userInformation)
        //{
        //    try
        //    {
        //        string defaultImage = "avatar-juan.png";
        //        string defaultPath = "/Images";
        //        DynamicParameters dParam = new DynamicParameters();
        //        if (userInformation.IsProfileImageClass == false)
        //        {
        //            dParam.Add("@ProfileImage", string.IsNullOrEmpty(userInformation.ProfileImage) ? defaultImage : userInformation.ProfileImage);
        //            dParam.Add("@ProfileImagePath", string.IsNullOrEmpty(userInformation.ProfileImagePath) ? defaultPath : userInformation.ProfileImagePath);
        //            dParam.Add("@ProfileImageClass", "");
        //        }
        //        else
        //        {
        //            dParam.Add("@ProfileImage", "");
        //            dParam.Add("@ProfileImagePath", "");
        //            dParam.Add("@ProfileImageClass", userInformation.ProfileImageClass);
        //        }
        //        dParam.Add("@UserID", UserID);
        //        userInformation = _dBFactory.InsertCommand_SPQuery(userInformation, "System_UserProfileImage_Insert", dParam);
        //        return userInformation;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        //public UserInformation DeleteUserProfile(Guid? UserID, Guid? accountId)
        //{
        //    UserInformation userInformation = new UserInformation();
        //    try
        //    {
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@UserID", UserID);
        //        dParam.Add("@AccountUserID", accountId);
        //        userInformation = _dBFactory.SelectCommand_SP(userInformation, "system_DeleteUserProfile", dParam);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return userInformation;
        //}

        #region FeedBack icons
        //public async Task<ResponseFeedback> InsertFeedBack(ResponseFeedback objmodel)
        //{
        //    ResponseFeedback result = new ResponseFeedback();
        //    DynamicParameters objpara = new DynamicParameters();
        //    try
        //    {
        //        objpara.Add("@UserID", objmodel.Userid);
        //        objpara.Add("@MessageID", objmodel.Messageid);
        //        objpara.Add("@Feedbacktype", objmodel.FeedbackType);
        //        result = await _dBFactory.InsertCommand_SPQueryAsync(result, "system_Insert_Feedback", objpara);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return result;
        //}

        /// <summary>
        /// To Insert reported message into DB.
        /// </summary>
        /// <param name="objmodel"></param>
        /// <returns></returns>
        //public async Task<ReportFeedback> InsertReportFeedback(ReportFeedback objmodel)
        //{
        //    ReportFeedback result = new ReportFeedback();
        //    DynamicParameters objpara = new DynamicParameters();
        //    try
        //    {
        //        objpara.Add("@MessageId", objmodel.MessageId);
        //        objpara.Add("@UserId", objmodel.UserId);
        //        objpara.Add("@SelectedReason", objmodel.SelectedReason);
        //        objpara.Add("@OptionalComment", objmodel.OptionalComment);
        //        result = await _dBFactory.InsertCommand_SPQueryAsync(result, "System_Insert_Report_Feedback", objpara);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return result;
        //}
        #endregion


        //public async Task<int> InsertProduct()
        //{
        //    try
        //    {
        //        var product = new Products
        //        {
        //            Name = "MacBook Air M3",
        //            Price = 120000,
        //            Stock = 10
        //        };
        //      int result = await _dBFactory.InsertAsync("Products", product);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return 1;
        //}

        /// <summary>
        /// Send OTP
        /// </summary>
        /// <param name="paymentDetails"></param>
        /// <param name="objSMTP"></param>
        /// <param name="DomainName"></param>
        /// <returns></returns>
        public async Task<int> Send_OTPToUser(StripePaymentDetails paymentDetails, _SMTPSetting objSMTP, string DomainName, string TemplatePath)
        {
            int retval = 0;
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (SmtpClient client = new SmtpClient(objSMTP.SMTPServer))
                {

                    #region Comment this region before publishing
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(objSMTP.SenderEmail, _cryptographyService.Decrypt(objSMTP.SenderPassword ?? ""));
                    #endregion
                    client.EnableSsl = objSMTP.SSlEnable;
                    client.Port = objSMTP.SMTPPort;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    using (MailMessage mailMessage = new MailMessage())
                    {
                        ////-----------------------Set From text according to domain name---------------------------
                        mailMessage.From = new MailAddress(objSMTP.SenderEmail!, objSMTP.DisplayName);
                        mailMessage.To.Add(paymentDetails.Email!);
                        mailMessage.Subject = "Unlock Your Songs Library – Use This Code";

                        var pathToFile = TemplatePath
                                  + "wwwroot"
                                  + Path.DirectorySeparatorChar.ToString()
                                  + "EmailTemplate" + Path.DirectorySeparatorChar.ToString()
                                  + "otpsongunlock.html";

                        StreamReader reader = new StreamReader(pathToFile);
                        string readFile = reader.ReadToEnd();

                        var random = new Random();
                        string code = random.Next(0, 10000).ToString("D4"); // always 4 chars

                        string StrContent = "";
                        StrContent = readFile;
                        mailMessage.Body = readFile;
                        if (mailMessage.Body != null)
                        {
                            mailMessage.Body = mailMessage.Body.ToString()
                                .Replace("<%DomainName%>", Convert.ToString(DomainName))
                                .Replace("<%UserName%>", Convert.ToString(paymentDetails.Name))
                                .Replace("<%AlbumName%>", Convert.ToString(paymentDetails.AlbumName))
                                .Replace("<%OTPCode%>", Convert.ToString(code))
                                .Replace("<%CurrentYear%>", Convert.ToString(System.DateTime.Now.Year));
                            mailMessage.IsBodyHtml = true;
                            //client.Send(mailMessage);
                            await client.SendMailAsync(mailMessage);
                            retval = int.Parse(code);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return retval;
        }

    }
}
