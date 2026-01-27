using BusinessLogic.IBusinessLogic.IContentManagerService;
using BusinessLogic.IBusinessLogic.ICryptoService;
using BusinessLogic.IBusinessLogic.IOpenAIClientService;
using BusinessLogic.IBusinessLogic.IWebsiteSettingsService;
using Dapper;
using DataAccess;
using DataTypes.ModelDataTypes.Account;
using DataTypes.ModelDataTypes.Common;
using DataTypes.ModelDataTypes.ContentManager;
using DataTypes.ModelDataTypes.Home;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;

namespace BusinessLogic.BLImplementation.ContentManagerService
{
    public class ContentManager : IContentManager
    {
        /// <summary>
        /// Purpose : Declare all possible objects
        /// </summary>
        private readonly ICryptographyService _cryptographyService;
        private DbFactory _dBFactory;
        private IWebsiteSettings _websiteSettings;
        private IOpenAIApiClientServices _openAIApiClientServices;

        /// <summary>
        /// Purpose :   Initialize constructure
        /// </summary>
        /// <param name="cryptographyService"></param>
        /// <param name="dBFactory"></param>
        /// <param name="websiteSettings"></param>
        public ContentManager(ICryptographyService cryptographyService, DbFactory dBFactory, IWebsiteSettings websiteSettings, IOpenAIApiClientServices openAIApiClientServices)
        {
            _cryptographyService = cryptographyService;
            _dBFactory = dBFactory;
            _websiteSettings = websiteSettings;
            _openAIApiClientServices = openAIApiClientServices;
        }

        #region JubileeBible

        //public List<JubileeBible> BindJubileeBibleAsync(string ID, string DomanName)
        //{
        //    try
        //    {
        //        List<JubileeBible> jubileeBibles = new List<JubileeBible>();
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@ID", ID);
        //        dParam.Add("@DomanName", DomanName);
        //        jubileeBibles = _dBFactory.SelectCommand_SP<JubileeBible>(jubileeBibles, "module_BibleStructure_Get", dParam);
        //        return jubileeBibles;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        /// <summary>
        /// Purpose :   Get book series into list
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="DomanName"></param>
        /// <returns></returns>
        //public List<JubileeBibleSeries> BindJubileeBibleSeriesAsync(string ID, string DomanName)
        //{
        //    try
        //    {
        //        List<JubileeBibleSeries> jubileeBibleSeries = new List<JubileeBibleSeries>();
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@ID", ID);
        //        dParam.Add("@DomanName", DomanName);
        //        jubileeBibleSeries = _dBFactory.SelectCommand_SP<JubileeBibleSeries>(jubileeBibleSeries, "module_BibleStructure_Get", dParam);
        //        return jubileeBibleSeries;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        /// <summary>
        /// Purpose :   Get book into list
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="DomanName"></param>
        /// <returns></returns>
        //public List<JubileeBibleBook> BindJubileeBibleBooksAsync(string ID, string DomanName)
        //{
        //    try
        //    {
        //        List<JubileeBibleBook> jubileeBibleBooks = new List<JubileeBibleBook>();
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@ID", ID);
        //        dParam.Add("@DomanName", DomanName);
        //        jubileeBibleBooks = _dBFactory.SelectCommand_SP<JubileeBibleBook>(jubileeBibleBooks, "module_BibleStructure_Get", dParam);
        //        return jubileeBibleBooks;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public async Task<JubileeBibleBook> BindJubileeBibleBookDataAsync(string ID)
        //{
        //    try
        //    {
        //        JubileeBibleBook jubileeBibleBook = new JubileeBibleBook();
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@ID", ID);
        //        jubileeBibleBook = await _dBFactory.SelectCommand_SPAsync(jubileeBibleBook, "module_BibleStructure_BibleBook_Get", dParam);
        //        return jubileeBibleBook;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        #endregion JubileeBible

        #region content-manager

        /// <summary>
        /// Purpose :   Get content into list
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="DomanName"></param>
        /// <returns></returns>
        //public List<ContentType> BindContentManagerAsync(string ID, string DomanName)
        //{
        //    try
        //    {
        //        List<ContentType> contentTypes = new List<ContentType>();
        //        DynamicParameters dParam = new DynamicParameters();
        //        //if (!string.IsNullOrEmpty(ID))
        //        //{
        //        dParam.Add("@ID", ID);
        //        dParam.Add("@DomanName", DomanName);
        //        contentTypes = _dBFactory.SelectCommand_SP<ContentType>(contentTypes, "module_ContentType_Get", dParam);
        //        //}
        //        //else
        //        //{
        //        //    dParam.Add("@DomanName", DomanName);
        //        //    contentTypes = _dBFactory.SelectCommand_SP<ContentType>(contentTypes, "module_ContentType_Get", dParam);
        //        //}
        //        return contentTypes;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        /// <summary>
        /// Purpose :   Get book series into list
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="DomanName"></param>
        /// <returns></returns>
        //public List<BookSeries> BindBookSeriesAsync(string ID, string DomanName)
        //{
        //    try
        //    {
        //        List<BookSeries> bookSeries = new List<BookSeries>();
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@ID", ID);
        //        dParam.Add("@DomanName", DomanName);
        //        bookSeries = _dBFactory.SelectCommand_SP<BookSeries>(bookSeries, "module_ContentType_Get", dParam);
        //        return bookSeries;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        /// <summary>
        /// Purpose :   Get book into list
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="DomanName"></param>
        /// <returns></returns>
        //public List<Book> BindBooksAsync(string ID, string DomanName)
        //{
        //    try
        //    {
        //        List<Book> books = new List<Book>();
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@ID", ID);
        //        dParam.Add("@DomanName", DomanName);
        //        books = _dBFactory.SelectCommand_SP<Book>(books, "module_ContentType_Get", dParam);
        //        return books;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        /// <summary>
        /// Purpose :  Get books list of level 1
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="DomanName"></param>
        /// <returns></returns>
        public List<BookLevel1> BindBooksLevel1Async(string ID, string DomanName)
        {
            try
            {
                List<BookLevel1> booksLevel1 = new List<BookLevel1>();
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@ID", ID);
                dParam.Add("@DomanName", DomanName);
                booksLevel1 = _dBFactory.SelectCommand_SP<BookLevel1>(booksLevel1, "module_ContentType_Get", dParam);
                return booksLevel1;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Purpose :  Save content into database
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="UserID"></param>
        /// <param name="DomanName"></param>
        /// <returns></returns>
        public async Task<Message> SaveContentType(string ContentName, string UserID, string DomanName)
        {
            try
            {
                Message message = new Message();
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@ContentName", ContentName.Trim());
                dParam.Add("@DomanName", DomanName.Trim());
                dParam.Add("@CreatedBy", UserID);
                message = _dBFactory.InsertCommand_SPQuery<Message>(message, "module_ContentType_Insert", dParam);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Purpose :  Save book series into database
        /// </summary>
        /// <param name="ContentTypeID"></param>
        /// <param name="BookSeriesName"></param>
        /// <param name="UserID"></param>
        /// <param name="DomanName"></param>
        /// <returns></returns>
        public async Task<Message> SaveBookSeriesAsync(string ContentTypeID, string BookSeriesName, string UserID, string DomanName)
        {
            try
            {
                Message message = new Message();
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@ID", ContentTypeID);
                dParam.Add("@ContentName", BookSeriesName.Trim());
                dParam.Add("@DomanName", DomanName.Trim());
                dParam.Add("@CreatedBy", UserID);
                message = _dBFactory.InsertCommand_SPQuery<Message>(message, "module_ContentType_Insert", dParam);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Purpose :  Save book content into database
        /// </summary>
        /// <param name="ContentTypeID"></param>
        /// <param name="ParentID"></param>
        /// <param name="Text"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public async Task<Message> SaveBookContentAsync(string ContentTypeID, string ParentID, string Text, string UserID)
        {
            try
            {
                Message message = new Message();
                DynamicParameters dParam = new DynamicParameters();
                if (!string.IsNullOrEmpty(ParentID))
                {
                    dParam.Add("@ParentID", ParentID);
                }
                dParam.Add("@ContentTypeID", ContentTypeID);
                dParam.Add("@Text", Text.Trim());
                dParam.Add("@CreatedBy", UserID);
                message = _dBFactory.InsertCommand_SPQuery<Message>(message, "module_BookContent_Insert", dParam);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Purpose :  Update table for backward and forward the content 
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Type"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public async Task<Message> BackwardForwardBookContentAsync(string ID, string Type, string UserID)
        {
            try
            {
                Message message = new Message();
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@ID", ID);
                dParam.Add("@Type", Type);
                dParam.Add("@CreatedBy", UserID);
                message = _dBFactory.InsertCommand_SPQuery<Message>(message, "module_BookContent_BackwardForward", dParam);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Purpose :  Bind book content using ContentTypeID
        /// </summary>
        /// <param name="ContentTypeID"></param>
        /// <param name="ParentID"></param>
        /// <returns></returns>
        public List<BookPart> BindBookContent_ContentTypeIDAsync(string ContentTypeID, string ParentID)
        {
            try
            {
                List<BookPart> bookParts = new List<BookPart>();
                DynamicParameters dParam = new DynamicParameters();
                int ChildCount = 0;
                if (!string.IsNullOrEmpty(ParentID))
                {
                    dParam.Add("@ContentTypeID", ContentTypeID);
                    dParam.Add("@ParentID", ParentID);
                }
                else
                {
                    dParam.Add("@ContentTypeID", ContentTypeID);
                }
                bookParts = _dBFactory.SelectCommand_SP<BookPart>(bookParts, "module_BookContent_Get", dParam);
                if (bookParts != null && bookParts.Count > 0)
                {
                    dParam = new DynamicParameters();
                    foreach (BookPart bookPart in bookParts)
                    {
                        dParam.Add("@ContentID", bookPart.BookContentID);
                        ChildCount = _dBFactory.SelectCommand_SP<int>(ChildCount, "module_BookContent_Child_Count_Get", dParam);
                        if (ChildCount > 0)
                        {
                            bookPart.ChildCount = ChildCount;
                        }
                        bookPart.bookIDList = GetBookContentIDList(bookPart.BookContentID, ParentID);
                    }
                }
                return bookParts;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Purpose :  Get saved Parent node using textbox value
        /// </summary>
        /// <param name="BookContentID"></param>
        /// <returns></returns>
        public async Task<List<BookPart>> BindSavedParentNodeUsingTextboxAsync(string BookContentID)
        {
            try
            {
                List<BookPart> bookParts = new List<BookPart>();
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@BookContentID", BookContentID);
                bookParts = await _dBFactory.SelectCommand_SP_List_Async<BookPart>(bookParts, "module_BookContent_BookContentID_Get", dParam);
                return bookParts;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Purpose :  Get book parts 
        /// </summary>
        /// <param name="ContentTypeID"></param>
        /// <param name="ParentID"></param>
        /// <returns></returns>
        public List<Guid> GetBookContentIDList(Guid ContentTypeID, string ParentID)
        {
            try
            {
                List<Guid> bookParts = new List<Guid>();
                DynamicParameters dParam = new DynamicParameters();
                dParam = new DynamicParameters();
                dParam.Add("@ParentID", ParentID);
                dParam.Add("@ContentTypeID", ContentTypeID);
                bookParts = _dBFactory.SelectCommand_SP<Guid>(bookParts, "module_BookContent_ParentID_Get", dParam);

                return bookParts;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Purpose :  
        /// </summary>
        /// <param name="ContentTypeID"></param>
        /// <param name="ParentID"></param>
        /// <returns></returns>
        public List<Guid> BindChildsBookContentIDByParentAsync(string ContentTypeID, string ParentID)
        {
            try
            {
                List<Guid> bookParts = new List<Guid>();
                DynamicParameters dParam = new DynamicParameters();
                dParam = new DynamicParameters();
                dParam.Add("@ParentID", ParentID);
                dParam.Add("@ContentTypeID", ContentTypeID);
                bookParts = _dBFactory.SelectCommand_SP<Guid>(bookParts, "module_BookContent_ContentID_Get", dParam);

                return bookParts;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Purpose :  
        /// </summary>
        /// <param name="ContentTypeID"></param>
        /// <param name="ParentID"></param>
        /// <returns></returns>
        public async Task<List<BookPart>> GetBookContent_ChildListAsync(string ContentTypeID, string ParentID)
        {
            try
            {
                List<BookPart> bookParts = new List<BookPart>();
                DynamicParameters dParam = new DynamicParameters();
                int ChildCount = 0;
                dParam = new DynamicParameters();
                dParam.Add("@ContentTypeID", ContentTypeID);
                dParam.Add("@ParentID", ParentID);
                bookParts = await _dBFactory.SelectCommand_SP_List_Async<BookPart>(bookParts, "module_BookContent_Child_Get", dParam);
                if (bookParts != null)
                {
                    foreach (BookPart bookPart in bookParts)
                    {
                        dParam = new DynamicParameters();
                        dParam.Add("@ContentID", bookPart.BookContentID);
                        ChildCount = await _dBFactory.SelectCommand_SPAsync<int>(ChildCount, "module_BookContent_Child_Count_Get", dParam);
                        if (ChildCount > 0)
                        {
                            bookPart.ChildCount = ChildCount;
                        }
                    }

                }
                return bookParts;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Purpose :  
        /// </summary>
        /// <param name="ContentTypeID"></param>
        /// <returns></returns>
        public string BindBookContentHeader_ContentTypeIDAsync(string ContentTypeID)
        {
            try
            {
                string NavigationText = "";
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@ContentTypeID", ContentTypeID);
                NavigationText = _dBFactory.SelectCommand_SP(NavigationText, "module_ContentType_NavigationText_Get", dParam);
                return NavigationText;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Purpose :  
        /// </summary>
        /// <param name="SubContentID"></param>
        /// <param name="Name"></param>
        /// <param name="Description"></param>
        /// <param name="ContentBody"></param>
        /// <returns></returns>
        public async Task<Message> SaveSubContentDetail(Guid SubContentID, string Name, string Description, string ContentBody)
        {
            try
            {
                Message message = new Message();
                DynamicParameters dParam = new DynamicParameters();

                dParam.Add("@SubContentID", SubContentID);
                dParam.Add("@Name", Name);
                dParam.Add("@Description", Description.Trim());
                dParam.Add("@ContentBody", ContentBody.Trim());

                message = _dBFactory.InsertCommand_SPQuery<Message>(message, "module_Books_Chapters_Verse_Add", dParam);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Purpose :  
        /// </summary>
        /// <param name="SubContentID"></param>
        /// <returns></returns>
        public async Task<SubContentModel> GetSubContentDetail(Guid SubContentID)
        {
            try
            {
                SubContentModel ObjSubContent = new SubContentModel();
                DynamicParameters dParam = new DynamicParameters();
                if (SubContentID != null)
                {
                    dParam.Add("@SubContentID", SubContentID);
                }

                ObjSubContent = _dBFactory.InsertCommand_SPQuery<SubContentModel>(ObjSubContent, "module_Books_Chapters_Verse_Get", dParam);
                return ObjSubContent;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Purpose :  
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Name"></param>
        /// <param name="Description"></param>
        /// <param name="ContentBody"></param>
        /// <returns></returns>
        public async Task<Message> UpdateSubContentDetail(Guid ID, string Name, string Description, string ContentBody)
        {
            try
            {
                Message message = new Message();
                DynamicParameters dParam = new DynamicParameters();

                dParam.Add("@ID", ID);
                dParam.Add("@Name", Name.Trim());
                dParam.Add("@Description", Description.Trim());
                dParam.Add("@ContentBody", ContentBody.Trim());

                message = _dBFactory.InsertCommand_SPQuery<Message>(message, "module_Books_Chapters_Verse_Update", dParam);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Purpose :  
        /// </summary>
        /// <param name="BookContentID"></param>
        /// <param name="ParentID"></param>
        /// <param name="siblingID"></param>
        /// <param name="Indent"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public async Task<Message> BookContentUpdateAsync(string BookContentID, string ParentID, string siblingID, int Indent, string Type)
        {
            try
            {
                Message message = new Message();
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@BookContentID", BookContentID);
                dParam.Add("@ParentID", ParentID);
                dParam.Add("@SiblingID", siblingID);
                dParam.Add("@Indent", Indent);
                dParam.Add("@Type", Type);
                message = await _dBFactory.InsertCommand_SPQueryAsync<Message>(message, "module_BookContent_BackwardForward_Update", dParam);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion content-manager

        #region ai-content-manager

        public async Task<List<CMSContentType>> GetCMSContentTypeAsync(string DomainName)
        {
            try
            {
                List<CMSContentType> cMSContentTypes = new List<CMSContentType>();
                DynamicParameters param = new DynamicParameters();
                param.Add("@DomainName", DomainName);
                cMSContentTypes = await _dBFactory.SelectCommand_SP_List_Async<CMSContentType>(cMSContentTypes, "cms_ContentType_Get", param);
                return cMSContentTypes;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> CheckBookContent(string ContentCode)
        {
            try
            {
                string retVal = string.Empty;
                DynamicParameters param = new DynamicParameters();
                param.Add("@ContentCode", ContentCode.Trim());
                retVal = await _dBFactory.SelectCommand_SPAsync(retVal, "module_BookContent_CheckBookCode", param);
                return retVal;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> GetColumnValueAsync(string ColumnName)
        {
            try
            {
                List<string> strings = new List<string>();
                string retVal = string.Empty;
                DynamicParameters param = new DynamicParameters();
                param.Add("@ColumnName", ColumnName);
                retVal = await _dBFactory.SelectCommand_SPAsync(retVal, "bookDetailsRawData_GetColumnData", param);
                return retVal;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Message> SaveContentDetailsAsync(string ContentTypeID, string ContentCode, string ContentTitle, string ContentType, string ContentCategory, string ContentTheme, string NarrativeStory, string NarrativeElements, string UserID, string filePath)
        {
            try
            {
                Message message = new Message();
                DynamicParameters param = new DynamicParameters();
                param.Add("@ContentTypeID", Guid.Parse(ContentTypeID));
                param.Add("@ContentCode", ContentCode.Trim());
                param.Add("@ContentTitle", ContentTitle.Trim());
                param.Add("@ContentType", ContentType.Trim());
                param.Add("@ContentCategory", ContentCategory.Trim());
                param.Add("@ContentTheme", ContentTheme.Trim());
                param.Add("@NarrativeStory", NarrativeStory.Trim());
                param.Add("@NarrativeElements", NarrativeElements.Trim());
                param.Add("@FilePath", filePath);
                param.Add("@CreatedBy", UserID);
                message = await _dBFactory.InsertCommand_SPQueryAsync<Message>(message, "module_BookContent_Save", param);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ContentDetails> BindContentDetailsAsync(string BookContentID)
        {
            try
            {
                ContentDetails contentDetails = new ContentDetails();
                DynamicParameters param = new DynamicParameters();
                param.Add("@BookContentID", BookContentID);
                contentDetails = await _dBFactory.SelectCommand_SPAsync(contentDetails, "module_BookContent_BindDetails", param);
                return contentDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<AIConfigurationSetting> BindAIConfigurationSettingsAsync(string ContentTypeID, string type)
        {
            try
            {
                AIConfigurationSetting aIConfigurationSetting = new AIConfigurationSetting();
                DynamicParameters param = new DynamicParameters();
                param.Add("@ContentTypeID", ContentTypeID);
                param.Add("@Type", type);
                aIConfigurationSetting = await _dBFactory.SelectCommand_SPAsync(aIConfigurationSetting, "bluePrint_Configuration_GetByType", param);
                return aIConfigurationSetting;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Message> SaveBluePrintResponseAsync(string BookContentID, string UserID, string responsePersonaContext, string responsePersonaRole, string responseNarrativeStructure, string responsePlotDevelopment, string responseThematicDepth, string responseBlueprintRating)
        {
            try
            {
                Message message = new Message();
                DynamicParameters param = new DynamicParameters();
                param.Add("@BookContentID", BookContentID);
                param.Add("@BluePrintPersonaContextResponse", responsePersonaContext == null ? null : FormatTextToHtml(responsePersonaContext));
                param.Add("@BluePrintPersonaRoleResponse", responsePersonaRole == null ? null : FormatTextToHtml(responsePersonaRole));
                param.Add("@BluePrintNarrativeStructureResponse", responseNarrativeStructure == null ? null : FormatTextToHtml(responseNarrativeStructure));
                param.Add("@BluePrintPlotDevelopmentResponse", responsePlotDevelopment == null ? null : FormatTextToHtml(responsePlotDevelopment));
                param.Add("@BluePrintThematicDepthResponse", responseThematicDepth == null ? null : FormatTextToHtml(responseThematicDepth));
                param.Add("@BluePrintBlueprintRatingResponse", responseBlueprintRating == null ? null : FormatTextToHtml(responseBlueprintRating));
                message = await _dBFactory.InsertCommand_SPQueryAsync<Message>(message, "cms_BookBluePrint_Save", param);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Message> SaveBlueprintContentAsync(string BookContentID, string UserID, BlueprintContent blueprintContent)
        {
            try
            {
                Message message = new Message();
                DynamicParameters param = new DynamicParameters();
                param.Add("@BookContentID", BookContentID);
                param.Add("@UserID", UserID);
                param.Add("@CharacterProfiles", blueprintContent.Character_Profiles == null ? null : FormatTextToHtml(blueprintContent.Character_Profiles));
                param.Add("@ProtagonistProfile", blueprintContent.Protagonist_Profile == null ? null : FormatTextToHtml(blueprintContent.Protagonist_Profile));
                param.Add("@AntagonistProfile", blueprintContent.Antagonist_Profile == null ? null : FormatTextToHtml(blueprintContent.Antagonist_Profile));
                param.Add("@MainPlot", blueprintContent.Main_Plot == null ? null : FormatTextToHtml(blueprintContent.Main_Plot));
                param.Add("@PlotPoints", blueprintContent.Plot_Points == null ? null : FormatTextToHtml(blueprintContent.Plot_Points));
                param.Add("@Subplots", blueprintContent.Subplots == null ? null : FormatTextToHtml(blueprintContent.Subplots));
                param.Add("@Themes", blueprintContent.Themes == null ? null : FormatTextToHtml(blueprintContent.Themes));
                param.Add("@Subthemes", blueprintContent.Subthemes == null ? null : FormatTextToHtml(blueprintContent.Subthemes));
                param.Add("@Setting", blueprintContent.Setting == null ? null : FormatTextToHtml(blueprintContent.Setting));
                param.Add("@Atmosphere", blueprintContent.Atmosphere == null ? null : FormatTextToHtml(blueprintContent.Atmosphere));
                param.Add("@Mysteries", blueprintContent.Mysteries == null ? null : FormatTextToHtml(blueprintContent.Mysteries));
                param.Add("@Clues", blueprintContent.Clues == null ? null : FormatTextToHtml(blueprintContent.Clues));
                param.Add("@Answers", blueprintContent.Answers == null ? null : FormatTextToHtml(blueprintContent.Answers));
                param.Add("@Foreshadowing", blueprintContent.Foreshadowing == null ? null : FormatTextToHtml(blueprintContent.Foreshadowing));
                param.Add("@Action", blueprintContent.Action == null ? null : FormatTextToHtml(blueprintContent.Action));
                param.Add("@Events", blueprintContent.Events == null ? null : FormatTextToHtml(blueprintContent.Events));
                param.Add("@Pacing", blueprintContent.Pacing == null ? null : FormatTextToHtml(blueprintContent.Pacing));
                param.Add("@Climax", blueprintContent.Climax == null ? null : FormatTextToHtml(blueprintContent.Climax));
                param.Add("@Twist", blueprintContent.Twist == null ? null : FormatTextToHtml(blueprintContent.Twist));
                param.Add("@Impact", blueprintContent.Impact == null ? null : FormatTextToHtml(blueprintContent.Impact));
                param.Add("@CentralConflict", blueprintContent.Central_Conflict == null ? null : FormatTextToHtml(blueprintContent.Central_Conflict));
                param.Add("@Resolution", blueprintContent.Resolution == null ? null : FormatTextToHtml(blueprintContent.Resolution));
                param.Add("@BookSummary", blueprintContent.Book_Summary == null ? null : FormatTextToHtml(blueprintContent.Book_Summary));
                param.Add("@ChapterOutline", blueprintContent.ChapterOutline == null ? null : FormatTextToHtml(blueprintContent.ChapterOutline));
                param.Add("@Act1", blueprintContent.Act1 == null ? null : FormatTextToHtml(blueprintContent.Act1));
                param.Add("@Act1Scenes", blueprintContent.Act1Scenes == null ? null : FormatTextToHtml(blueprintContent.Act1Scenes));
                param.Add("@Act2P1", blueprintContent.Act2P1 == null ? null : FormatTextToHtml(blueprintContent.Act2P1));
                param.Add("@Act2P1Scenes", blueprintContent.Act2P1Scenes == null ? null : FormatTextToHtml(blueprintContent.Act2P1Scenes));
                param.Add("@Act2P2", blueprintContent.Act2P2 == null ? null : FormatTextToHtml(blueprintContent.Act2P2));
                param.Add("@Act2P2Scenes", blueprintContent.Act2P2Scenes == null ? null : FormatTextToHtml(blueprintContent.Act2P2Scenes));
                param.Add("@Act3", blueprintContent.Act3 == null ? null : FormatTextToHtml(blueprintContent.Act3));
                param.Add("@Act3Scenes", blueprintContent.Act3Scenes == null ? null : FormatTextToHtml(blueprintContent.Act3Scenes));
                param.Add("@ThreeActBT01", blueprintContent.ThreeActBT01 == null ? null : FormatTextToHtml(blueprintContent.ThreeActBT01));
                param.Add("@ThreeActBT02", blueprintContent.ThreeActBT02 == null ? null : FormatTextToHtml(blueprintContent.ThreeActBT02));
                param.Add("@ThreeActBT03", blueprintContent.ThreeActBT03 == null ? null : FormatTextToHtml(blueprintContent.ThreeActBT03));
                param.Add("@ThreeActBT04", blueprintContent.ThreeActBT04 == null ? null : FormatTextToHtml(blueprintContent.ThreeActBT04));
                param.Add("@ThreeActBT05", blueprintContent.ThreeActBT05 == null ? null : FormatTextToHtml(blueprintContent.ThreeActBT05));
                param.Add("@ThreeActBT06", blueprintContent.ThreeActBT06 == null ? null : FormatTextToHtml(blueprintContent.ThreeActBT06));
                param.Add("@ThreeActBT07", blueprintContent.ThreeActBT07 == null ? null : FormatTextToHtml(blueprintContent.ThreeActBT07));
                param.Add("@ThreeActBT08", blueprintContent.ThreeActBT08 == null ? null : FormatTextToHtml(blueprintContent.ThreeActBT08));
                param.Add("@ThreeActBT09", blueprintContent.ThreeActBT09 == null ? null : FormatTextToHtml(blueprintContent.ThreeActBT09));
                param.Add("@ThreeActBT10", blueprintContent.ThreeActBT10 == null ? null : FormatTextToHtml(blueprintContent.ThreeActBT10));
                param.Add("@FifteenBeat01", blueprintContent.FifteenBeat01 == null ? null : FormatTextToHtml(blueprintContent.FifteenBeat01));
                param.Add("@FifteenBeat02", blueprintContent.FifteenBeat02 == null ? null : FormatTextToHtml(blueprintContent.FifteenBeat02));
                param.Add("@FifteenBeat03", blueprintContent.FifteenBeat03 == null ? null : FormatTextToHtml(blueprintContent.FifteenBeat03));
                param.Add("@FifteenBeat04", blueprintContent.FifteenBeat04 == null ? null : FormatTextToHtml(blueprintContent.FifteenBeat04));
                param.Add("@FifteenBeat05", blueprintContent.FifteenBeat05 == null ? null : FormatTextToHtml(blueprintContent.FifteenBeat05));
                param.Add("@FifteenBeat06", blueprintContent.FifteenBeat06 == null ? null : FormatTextToHtml(blueprintContent.FifteenBeat06));
                param.Add("@FifteenBeat07", blueprintContent.FifteenBeat07 == null ? null : FormatTextToHtml(blueprintContent.FifteenBeat07));
                param.Add("@FifteenBeat08", blueprintContent.FifteenBeat08 == null ? null : FormatTextToHtml(blueprintContent.FifteenBeat08));
                param.Add("@FifteenBeat09", blueprintContent.FifteenBeat09 == null ? null : FormatTextToHtml(blueprintContent.FifteenBeat09));
                param.Add("@FifteenBeat10", blueprintContent.FifteenBeat10 == null ? null : FormatTextToHtml(blueprintContent.FifteenBeat10));
                param.Add("@FifteenBeat11", blueprintContent.FifteenBeat11 == null ? null : FormatTextToHtml(blueprintContent.FifteenBeat11));
                param.Add("@FifteenBeat12", blueprintContent.FifteenBeat12 == null ? null : FormatTextToHtml(blueprintContent.FifteenBeat12));
                param.Add("@FifteenBeat13", blueprintContent.FifteenBeat13 == null ? null : FormatTextToHtml(blueprintContent.FifteenBeat13));
                param.Add("@FifteenBeat14", blueprintContent.FifteenBeat14 == null ? null : FormatTextToHtml(blueprintContent.FifteenBeat14));
                param.Add("@FifteenBeat15", blueprintContent.FifteenBeat15 == null ? null : FormatTextToHtml(blueprintContent.FifteenBeat15));
                param.Add("@Chapter", blueprintContent.Chapter);
                param.Add("@ChapterScene", blueprintContent.ChapterScene);

                message = await _dBFactory.InsertCommand_SPQueryAsync<Message>(message, "BlueprintContent_Save", param);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string FormatTextToHtml(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }
            // Add paragraph breaks for consecutive newlines
            text = Regex.Replace(text, @"(\r\n?|\n)+", "<br>");
            //text = Regex.Replace(text, @"(\r\n?|\n)+", "</p><p>");
            // Add <p> tags to the beginning and end of the text
            //text = $"<p>{text}</p>";
            //text = $"<p>{text}<div class='dot-cursor'></div></p>";
            // Add <b> tags for text enclosed in asterisks (e.g., bold)
            text = Regex.Replace(text, @"\*(.*?)\*", "<b>$1</b>");
            return text;
        }

        public async Task<Message> DeleteContentDetailsAsync(string BookContentID)
        {
            try
            {
                Message message = new Message();
                DynamicParameters param = new DynamicParameters();
                param.Add("@BookContentID", BookContentID);
                message = await _dBFactory.SelectCommand_SPAsync(message, "module_BookContent_Delete", param);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Message> SaveActChaptersAsync(string ParentID, string ContentTypeID, string ActTitle, string ActTitleWithHtml, string UserID, string type)
        {
            try
            {
                Message message = new Message();
                DynamicParameters param = new DynamicParameters();
                param.Add("@ParentID", ParentID);
                param.Add("@ContentTypeID", ContentTypeID);
                param.Add("@ActChapterTitle", ActTitle);
                param.Add("@ActChapterTitleWithHtml", ActTitleWithHtml);
                param.Add("@Type", type);
                param.Add("@UserID", UserID);
                message = await _dBFactory.SelectCommand_SPAsync(message, "module_BookContent_ActChapterSave", param);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Message> UpdateBlueprintDataAsync(string BookContentID, string BlueprintData, string UserID)
        {
            try
            {
                Message message = new Message();
                DynamicParameters param = new DynamicParameters();
                param.Add("@BookContentID", BookContentID);
                param.Add("@BlueprintData", BlueprintData);
                param.Add("@UserID", UserID);
                message = await _dBFactory.SelectCommand_SPAsync(message, "module_BookContent_UpdateBlueprint", param);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Purpose :  Bind model from Dictionary key value data 
        /// </summary>
        /// <param name="bookData"></param>
        /// <returns></returns>
        public BookContentCode GetBookContentCodeModelFromDictionary(Dictionary<string, string> bookData)
        {
            BookContentCode bookContent = new BookContentCode();
            try
            {
                foreach (var entry in bookData)
                {
                    switch (entry.Key.ToLower())
                    {
                        case "content code":
                            bookContent.ContentCode = entry.Value;
                            break;
                        case "book idea":
                            bookContent.BookIdea = entry.Value;
                            break;
                        case "book title":
                            bookContent.BookTitle = entry.Value;
                            break;
                        case "book theme":
                            bookContent.BookTheme = entry.Value;
                            break;
                        case "book goals":
                            bookContent.BookGoals = entry.Value;
                            break;

                        case "book persona":
                            bookContent.BookPersona = entry.Value;
                            break;
                            // Handle additional properties if needed
                    }
                }
                return bookContent;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<ChapterNonFiction>> ExtractChapterSectionNonFiction(string response, string AIChapterSections)
        {
            List<ChapterNonFiction> chapters = new List<ChapterNonFiction>();
            try
            {
                // Define regular expressions for chapters and sections
                Regex chapterRegex = new Regex(@"(\d+)\.\s+(.+)");
                Regex sectionRegex = new Regex(@"(\d+\.\d+)\s+(.+)");

                // Match chapters and sections
                var chaptersMatches = chapterRegex.Matches(response);
                var sectionsMatches = sectionRegex.Matches(response);

                string ChapterSections = string.Empty;
                // Create a new chapter and section objects and store them in the list
                foreach (Match match in chaptersMatches)
                {
                    ChapterSections = match.Groups[2].Value.Trim();
                    ///Maintain chapters for blueprint data.
                    var chapter = new ChapterNonFiction
                    {
                        Title = "Chapter " + match.Groups[0].Value.Trim(),
                        Sections = new List<SectionNonFiction>()
                    };

                    chapters.Add(chapter);

                    // Find sections corresponding to this chapter
                    foreach (Match sectionMatch in sectionsMatches)
                    {
                        if (sectionMatch.Groups[1].Value.StartsWith(match.Groups[1].Value + ".")) // Add dot to ensure correct matching
                        {
                            ChapterSections += " " + sectionMatch.Groups[0].Value.Trim();
                            chapter.Sections.Add(new SectionNonFiction
                            {
                                Title = "Section " + sectionMatch.Groups[0].Value.Trim()
                            });
                        }
                    }

                    /// Get all Chapters and there sections from GPT4.0
                    if (!string.IsNullOrEmpty(ChapterSections))
                    {
                        try
                        {
                            string chapterSec = await _openAIApiClientServices.SendPromptUsingAzureGpt4Async(AIChapterSections + "\n\n" + ChapterSections, "");
                            // string chapterSec = "Chapter 7\r\nCultural Power Dynamics Reimagined\r\n\r\nSection 7.1\r\nDecoding Power in Varied Cultural Landscapes.\r\nIn this section, we delve into the intricate fabric of power as perceived and exercised in different cultural contexts. Power, inherently a social construct, varies significantly across cultures. Its manifestations and the reactions it elicits are deeply rooted in the specific values, norms, and historical backgrounds of each society. A comprehensive understanding of these diverse power structures is crucial for effective intercultural interaction. Through an exploration of case studies and theoretical frameworks, this section seeks to elucidate the multifaceted nature of power. We examine how different societies organize hierarchy and authority, and the interplay between power, status, and influence. The discussion extends to the subtleties of power dynamics, including the often-overlooked informal power that operates alongside or even outside of official structures. This nuanced understanding of power across cultures is imperative for anyone navigating the global landscape, whether in diplomacy, international business, or cross-cultural partnerships.\r\n\r\nSection 7.2\r\nCultural Values Shaping Leadership.\r\nThe focus of this section is the profound impact of underlying cultural values on leadership styles and practices. Culture significantly influences what is considered effective leadership, from the way decisions are made to how authority is displayed and respected. We dissect the various dimensions of leadership, such as collectivism versus individualism, power distance, masculinity versus femininity, uncertainty avoidance, long-term orientation, and indulgence versus restraint. By comparing and contrasting leadership styles across different cultures, such as the participative approach commonly seen in Scandinavian countries versus the more hierarchical style prevalent in many Asian cultures, readers will gain insights into the complexities of leading in a multicultural environment. This section also explores how leaders can adapt their style to resonate with the cultural values of their teams, thus enhancing their effectiveness and fostering a more inclusive workplace. Real-world examples of leaders who have successfully adapted their approach to different cultural contexts will be highlighted, providing practical models for readers to emulate.\r\n\r\nSection 7.3\r\nMastering Cross-Cultural Power Dynamics.\r\nNavigating cross-cultural power dynamics is a delicate art that requires keen awareness and strategic finesse. This section explores the challenges and opportunities presented when individuals or organizations engage across cultures with varying perceptions of power. We discuss strategies for recognizing and respecting different power hierarchies while also asserting one's own position effectively. The interplay between explicit authority and the unspoken rules of engagement is examined, with particular attention given to understanding and leveraging 'soft power'—the ability to attract and co-opt rather than coerce. We consider the role of communication styles, conflict resolution approaches, and negotiation tactics in cross-cultural settings. By providing concrete tools and methods, this section aims to equip readers with the skills to build mutually beneficial relationships that respect the power dynamics of all parties involved.\r\n\r\nSection 7.4\r\nCultivating Cultural Sensitivity and Adaptability.\r\nCultural sensitivity and adaptability are key competencies in today's interconnected world. This section emphasizes the importance of these skills in managing cultural power dynamics effectively. We explore the concept of cultural intelligence and its components—cognitive, emotional, and physical—which are essential for operating with respect and understanding in diverse settings. Readers will learn how to develop empathy for different cultural perspectives and behaviors, as well as how to adjust their own responses accordingly. The section provides practical advice on avoiding cultural faux pas, understanding the role of context in communication, and the importance of patience and openness to different ways of thinking and working. By fostering cultural sensitivity and adaptability, individuals and organizations can create an environment where all members feel valued and understood, leading to more harmonious and productive interactions.\r\n\r\nSection 7.5\r\nBridging Cultural Divides with Strategy.\r\nBuilding bridges across cultural divides is not just a moral imperative but a strategic necessity. In this section, we present strategies for fostering understanding and cooperation between individuals from diverse cultural backgrounds. We discuss the significance of active listening, empathy, and the creation of shared goals as foundational to bridging cultural gaps. The role of intercultural training and education in preparing individuals for cross-cultural interactions is examined, along with the benefits of cultural exchange programs and initiatives. We also consider the impact of technology in facilitating intercultural communication and collaboration. Through a combination of theory and practical examples, this section aims to provide readers with actionable strategies for overcoming cultural barriers and building lasting partnerships.\r\n\r\nSection 7.6\r\nDiversity, Inclusion, and Ethical Power.\r\nThe role of diversity and inclusion in creating ethical power structures within organizations and societies is the central theme of this section. We examine how inclusive practices can lead to more equitable and just power dynamics that respect and harness the strengths of diverse groups. This exploration includes a discussion on the barriers to diversity and inclusion, such as unconscious bias and systemic discrimination, and how they can be addressed. The section also delves into the benefits of diverse perspectives in decision-making processes and the positive impact on innovation and performance. By highlighting best practices and success stories, we demonstrate how fostering diversity and inclusion can transform power structures into more ethical and effective systems.\r\n\r\nSection 7.7\r\nBiblical Insights on Cultural Differences.\r\nIn this concluding section, we explore the guidance offered by Biblical principles for navigating cultural differences and power dynamics. The section reflects on the teachings of respect, love, and unity as foundational to understanding and interacting with diverse cultures. We draw connections between Biblical narratives and modern-day intercultural encounters, offering insights into how ancient wisdom can inform contemporary practices. The discussion includes an examination of the concept of servant leadership as a model for exercising power ethically and responsibly in a multicultural context. Through a thoughtful integration of scripture and cultural analysis, this section aims to provide a spiritual perspective on the challenges and rewards of engaging with the rich tapestry of global cultures.\r\n";
                            if (!string.IsNullOrEmpty(chapterSec))
                            {
                                // Pattern to match sections and capture their titles
                                var sectionPatternWithSectionNewline = @"(Section\s(\d+\.\d+)\s(.*?)(?=\nSection\s\d+\.\d+|\Z))";
                                var sectionPatternWithSection = @"(Section\s+\d+\.\d+:\s+.*?)(?=\s+Section\s+\d+\.\d+:|\s*$)";
                                string sectionPattern = @"(\d+\.\d+ [^\n]+)\n(.*?)(?=\n\d+\.\d+|\z)";
                                // Using RegexOptions.Single line to make . match newline characters as well
                                var secMatchesSec = Regex.Matches(chapterSec, sectionPatternWithSection, RegexOptions.Singleline);
                                var secMatchesSecNewline = Regex.Matches(chapterSec, sectionPatternWithSectionNewline, RegexOptions.Singleline);
                                var secMatches = Regex.Matches(chapterSec, sectionPattern, RegexOptions.Singleline);

                                List<SectionNonFiction> sectionsNew = new List<SectionNonFiction>();
                                if (secMatchesSec.Count > 0)
                                {
                                    foreach (Match matchSec in secMatchesSec)
                                    {
                                        sectionsNew.Add(new SectionNonFiction
                                        {
                                            Title = matchSec.Groups[1].Value.Trim()
                                        });
                                    }
                                }
                                else if (secMatchesSecNewline.Count > 0)
                                {
                                    foreach (Match matchSec in secMatchesSecNewline)
                                    {
                                        sectionsNew.Add(new SectionNonFiction
                                        {
                                            Title = matchSec.Groups[1].Value.Trim()
                                        });
                                    }
                                }
                                else if (secMatches.Count > 0)
                                {
                                    foreach (Match matchSec in secMatches)
                                    {
                                        sectionsNew.Add(new SectionNonFiction
                                        {
                                            Title = "Section " + matchSec.Groups[1].Value.Trim()
                                        });
                                    }
                                }
                                if (sectionsNew.Any())
                                {
                                    chapter.Sections = sectionsNew;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _websiteSettings.InsertErrorLogs(Guid.Empty, "ContentManager", "Calling of AzureGpt4Async", ex.Message, ex.Message, "Frontend", "", true);
                        }
                    }
                }
                return chapters;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Message> UpdateBlueprintAsync(string UserID, string BookContentID, string BlueprintData)
        {
            try
            {
                Message message = new Message();
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@BookContentID", BookContentID);
                dParam.Add("@BlueprintData", BlueprintData);
                dParam.Add("@UserID", UserID);
                message = await _dBFactory.InsertCommand_SPQueryAsync<Message>(message, "BookBluePrint_InsertUpdate", dParam);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<ChapterNonFiction>> BindChapterOutlinesAsync(string BookContentID)
        {
            try
            {
                List<ChapterNonFiction> chapterNonFictions = new List<ChapterNonFiction>();
                DynamicParameters param = new DynamicParameters();
                param.Add("@BookContentID", BookContentID);
                chapterNonFictions = await _dBFactory.SelectCommand_SP_List_Async<ChapterNonFiction>(chapterNonFictions, "module_BookContent_ChapterOutlines", param);
                return chapterNonFictions;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<SectionNonFiction>> BindSectionOutlinesAsync(string BookContentID)
        {
            try
            {
                List<SectionNonFiction> sectionNonFictions = new List<SectionNonFiction>();
                DynamicParameters param = new DynamicParameters();
                param.Add("@BookContentID", BookContentID);
                sectionNonFictions = await _dBFactory.SelectCommand_SP_List_Async<SectionNonFiction>(sectionNonFictions, "module_BookContent_ChapterOutlines", param);
                return sectionNonFictions;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> SaveBookIntrodution(string BookContentID, string Description, string UserID)
        {
            try
            {
                string retVal = string.Empty;
                DynamicParameters param = new DynamicParameters();
                param.Add("@BookContentID", BookContentID);
                param.Add("@Description", Description);
                param.Add("@UserID", UserID);
                retVal = await _dBFactory.SelectCommand_SPAsync(retVal, "module_BookIntroduction_Save", param);
                return retVal;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<BookIntroduction> BindContentData_Async(string BookContentID)
        {
            try
            {
                BookIntroduction bookintroduction = new BookIntroduction();
                DynamicParameters param = new DynamicParameters();
                param.Add("@BookContentID", BookContentID);
                bookintroduction = await _dBFactory.SelectCommand_SPAsync(bookintroduction, "module_BookIntroduction_Get", param);
                return bookintroduction;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion ai-content-manager

        #region book-writer

        /// <summary>
        /// Purpose :  Get all content data into key value form
        /// </summary>
        /// <param name="content"></param>
        /// <param name="identifiers"></param>
        /// <returns></returns>
        public Dictionary<string, string> ExtractBookData(string content, HashSet<string> identifiers)
        {
            /// Dictionary to hold the extracted data
            var extractedData = new Dictionary<string, string>();
            /// Read each line from the file
            if (!string.IsNullOrEmpty(content))
            {
                using (StringReader reader = new StringReader(content))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            /// Split the line into key and value at the first occurrence of ":"
                            var index = line.IndexOf(':');
                            if (index != -1)
                            {
                                var key = line.Substring(0, index).Trim();
                                var value = line.Substring(index + 1).Trim().Trim('"'); /// Remove leading/trailing spaces and quotation marks
                                                                                        /// Check if the key is one of the identifiers we're looking for
                                if (identifiers.Contains(key.ToLower()))
                                {
                                    extractedData[key] = value;
                                }
                            }
                        }
                    }
                }
            }
            return extractedData;
        }

        /// <summary>
        /// Purpose :  Get all multiline content data into key value form
        /// </summary>
        /// <param name="content"></param>
        /// <param name="identifiers"></param>
        /// <returns></returns>
        public Dictionary<string, string> ExtractBookDataFull(string content, HashSet<string> identifiers)
        {
            /// Declare a dictionary 
            var extractedData = new Dictionary<string, string>();
            try
            {
                string? currentKey = null;
                string? currentValue = null;
                if (!string.IsNullOrEmpty(content))
                {
                    /// Use StringReader to read the line from string
                    using (StringReader reader = new StringReader(content))
                    {
                        string? line;
                        /// Read each line from the file
                        while ((line = reader.ReadLine()) != null)
                        {
                            /// Check if line has data
                            if (!string.IsNullOrEmpty(line))
                            {
                                /// Get first index of (:)
                                var index = line.IndexOf(':');
                                if (index != -1)
                                {
                                    var key = line.Substring(0, index);
                                    var value = line.Substring(index + 1);
                                    /// If contains key
                                    if (identifiers.Contains(key.ToLower()))
                                    {
                                        /// If a new identifier is found, save the previous one (if any) and reset
                                        if (currentKey != null)
                                        {
                                            extractedData[currentKey] = Convert.ToString(currentValue) ?? "";
                                        }
                                        /// Start processing the new identifier
                                        currentKey = Convert.ToString(key).Trim().Trim('"');
                                        currentValue = Convert.ToString(value).Trim().Trim('"');
                                    }
                                    else
                                    {
                                        /// If the line doesn't contain an identifier, append it to the current value
                                        currentValue += Environment.NewLine + Convert.ToString(value);
                                    }
                                }
                                else
                                {
                                    /// If the line doesn't contain a colon, append it to the current value
                                    currentValue += Environment.NewLine + line.Trim().Trim('"');
                                }
                            }
                        }
                    }
                }

                /// Save the last identifier (if any)
                if (currentKey != null)
                {
                    extractedData[currentKey] = currentValue;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading the file: {ex.Message}");
            }

            return extractedData;
        }

        /// <summary>
        /// Purpose :  Bind model from Dictionary key value data 
        /// </summary>
        /// <param name="bookData"></param>
        /// <returns></returns>
        public BookContent GetBookModelFromDictionary(Dictionary<string, string> bookData)
        {
            BookContent bookContent = new BookContent();
            try
            {
                foreach (var entry in bookData)
                {
                    switch (entry.Key.ToLower())
                    {
                        case "book code":
                            bookContent.BookCode = entry.Value;
                            break;
                        case "book title":
                            bookContent.BookTitle = entry.Value;
                            break;
                        case "publisher":
                            bookContent.Publisher = entry.Value;
                            break;
                        case "number of chapters":
                            int numberOfChapters;
                            if (int.TryParse(entry.Value, out numberOfChapters))
                            {
                                bookContent.NumberOfChapters = numberOfChapters;
                            }
                            break;
                        case "book theme":
                            bookContent.BookTheme = entry.Value;
                            break;
                            // Handle additional properties if needed
                    }
                }
                return bookContent;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Purpose :  Get all properties of model into HashSet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static HashSet<string> GetPropertyNamesInHashSet<T>(T instance)
        {
            Type type = typeof(T);

            // Get all public properties of the type
            var properties = type.GetProperties();

            // Extract property names into a HashSet
            HashSet<string> propertyNames = new HashSet<string>();
            foreach (var property in properties)
            {
                propertyNames.Add(property.Name);
            }

            return propertyNames;
        }

        /// <summary>
        /// Purpose :  Get all properties of model into List of string type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public List<string> GetPropertyNamesInList<T>(T instance)
        {
            Type type = typeof(T);

            // Get all public properties of the type
            var properties = type.GetProperties();

            // Extract property names into a list
            List<string> propertyNames = new List<string>();
            foreach (var property in properties)
            {
                propertyNames.Add(property.Name);
            }

            return propertyNames;
        }

        /// <summary>
        /// Purpose :  Save/Update model data into database
        /// </summary>
        /// <param name="bookContent"></param>
        /// <param name="UserID"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<Message> SaveUpdateBookAsync(BookContent bookContent, string UserID, string fileName)
        {
            try
            {
                Message message = new Message();
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@BookCode", bookContent.BookCode);
                dParam.Add("@BookTitle", bookContent.BookTitle);
                dParam.Add("@Publisher", bookContent.Publisher);
                dParam.Add("@NumberOfChapters", bookContent.NumberOfChapters);
                dParam.Add("@BookTheme", bookContent.BookTheme);
                dParam.Add("@FileName", fileName);
                dParam.Add("@UserID", UserID);
                message = await _dBFactory.InsertCommand_SPQueryAsync<Message>(message, "BookBluePrint_InsertUpdate", dParam);
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion book-writer

       
    }
}
