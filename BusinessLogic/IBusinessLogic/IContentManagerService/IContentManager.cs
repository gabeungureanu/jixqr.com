using DataTypes.ModelDataTypes.Common;
using DataTypes.ModelDataTypes.ContentManager;
using DataTypes.ModelDataTypes.Home;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.IBusinessLogic.IContentManagerService
{
    public interface IContentManager
    {
        #region JubileeBibleBooks
        //List<JubileeBible> BindJubileeBibleAsync(string ID, string DomanName);
        //List<JubileeBibleSeries> BindJubileeBibleSeriesAsync(string ID, string DomanName);
        //List<JubileeBibleBook> BindJubileeBibleBooksAsync(string ID, string DomanName);
        //Task<JubileeBibleBook> BindJubileeBibleBookDataAsync(string ID);

        #endregion JubileeBibleBooks


        #region content-manager
        /// <summary>
        /// Purpose :   Get content into list
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="DomanName"></param>
        /// <returns></returns>
        //List<ContentType> BindContentManagerAsync(string ID, string DomanName);
        /// <summary>
        /// Purpose :   Get book series into list
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="DomanName"></param>
        /// <returns></returns>
        //List<BookSeries> BindBookSeriesAsync(string ID, string DomanName);
        /// <summary>
        /// Purpose :   Get book into list
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="DomanName"></param>
        /// <returns></returns>
        //List<Book> BindBooksAsync(string ID, string DomanName);
        /// <summary>
        /// Purpose :  Get books list of level 1
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="DomanName"></param>
        /// <returns></returns>
        List<BookLevel1> BindBooksLevel1Async(string ID, string DomanName);
        /// <summary>
        /// Purpose :  Save content into database
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="UserID"></param>
        /// <param name="DomanName"></param>
        /// <returns></returns>
        Task<Message> SaveContentType(string ContentName, string UserID, string DomanName);
        /// <summary>
        /// Purpose :  Save book series into database
        /// </summary>
        /// <param name="ContentTypeID"></param>
        /// <param name="BookSeriesName"></param>
        /// <param name="UserID"></param>
        /// <param name="DomanName"></param>
        /// <returns></returns>
        Task<Message> SaveBookSeriesAsync(string ContentTypeID, string BookSeriesName, string UserID, string DomanName);
        /// <summary>
        /// Purpose :  Save book content into database
        /// </summary>
        /// <param name="ContentTypeID"></param>
        /// <param name="ParentID"></param>
        /// <param name="Text"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        Task<Message> SaveBookContentAsync(string ContentTypeID, string ParentID, string Text, string UserID);
        /// <summary>
        /// Purpose :  Update table for backward and forward the content 
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Type"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        Task<Message> BackwardForwardBookContentAsync(string ID, string Type, string UserID);
        /// <summary>
        /// Purpose :  Bind book content using ContentTypeID
        /// </summary>
        /// <param name="ContentTypeID"></param>
        /// <param name="ParentID"></param>
        /// <returns></returns>
        List<BookPart> BindBookContent_ContentTypeIDAsync(string ContentTypeID, string ParentID);
        /// <summary>
        /// Purpose :  
        /// </summary>
        /// <param name="ContentTypeID"></param>
        /// <returns></returns>
        string BindBookContentHeader_ContentTypeIDAsync(string ContentTypeID);
        /// <summary>
        /// Purpose :  
        /// </summary>
        /// <param name="SubContentID"></param>
        /// <param name="Name"></param>
        /// <param name="Description"></param>
        /// <param name="ContentBody"></param>
        /// <returns></returns>
        Task<Message> SaveSubContentDetail(Guid SubContentID, string Name, string Description, string ContentBody);
        /// <summary>
        /// Purpose :  
        /// </summary>
        /// <param name="SubContentID"></param>
        /// <returns></returns>
        Task<SubContentModel> GetSubContentDetail(Guid SubContentID);
        /// <summary>
        /// Purpose :  
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Name"></param>
        /// <param name="Description"></param>
        /// <param name="ContentBody"></param>
        /// <returns></returns>
        Task<Message> UpdateSubContentDetail(Guid ID, string Name, string Description, string ContentBody);
        /// <summary>
        /// Purpose :  Get book parts 
        /// </summary>
        /// <param name="ContentTypeID"></param>
        /// <param name="ParentID"></param>
        /// <returns></returns>
        List<Guid> GetBookContentIDList(Guid ContentTypeID, string ParentID);
        /// <summary>
        /// Purpose :  
        /// </summary>
        /// <param name="ContentTypeID"></param>
        /// <param name="ParentID"></param>
        /// <returns></returns>
        Task<List<BookPart>> GetBookContent_ChildListAsync(string ContentTypeID, string ParentID);
        /// <summary>
        /// Purpose :  Get saved Parent node using textbox value
        /// </summary>
        /// <param name="BookContentID"></param>
        /// <returns></returns>
        Task<List<BookPart>> BindSavedParentNodeUsingTextboxAsync(string BookContentID);
        /// <summary>
        /// Purpose :  
        /// </summary>
        /// <param name="ContentTypeID"></param>
        /// <param name="ParentID"></param>
        /// <returns></returns>
        List<Guid> BindChildsBookContentIDByParentAsync(string ContentTypeID, string ParentID);
        /// <summary>
        /// Purpose :  
        /// </summary>
        /// <param name="BookContentID"></param>
        /// <param name="ParentID"></param>
        /// <param name="siblingID"></param>
        /// <param name="Indent"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        Task<Message> BookContentUpdateAsync(string BookContentID, string ParentID, string siblingID, int Indent, string type);
        #endregion content-manager

        #region ai-content-manager
        /// <summary>
        /// Purpose :   Bond Content Type
        /// </summary>
        /// <param name="DomainName"></param>
        /// <returns></returns>
        Task<List<CMSContentType>> GetCMSContentTypeAsync(string DomainName);
        /// <summary>
        /// Purpose :   Insert Content details into database
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="ContentTypeID"></param>
        /// <param name="ContentCode"></param>
        /// <param name="ContentTitle"></param>
        /// <param name="ContentType"></param>
        /// <param name="ContentCategory"></param>
        /// <param name="ContentTheme"></param>
        /// <param name="NarrativeStory"></param>
        /// <param name="NarrativeElements"></param>
        /// <returns></returns>
        Task<Message> SaveContentDetailsAsync(string ContentTypeID, string ContentCode, string ContentTitle, string ContentType, string ContentCategory, string ContentTheme, string NarrativeStory, string NarrativeElements, string UserID, string filePath);

        Task<ContentDetails> BindContentDetailsAsync(string BookContentID);

        Task<AIConfigurationSetting> BindAIConfigurationSettingsAsync(string ContentTypeID, string type);

        Task<Message> DeleteContentDetailsAsync(string BookContentID);

        BookContentCode GetBookContentCodeModelFromDictionary(Dictionary<string, string> bookData);

        Task<List<ChapterNonFiction>> ExtractChapterSectionNonFiction(string response, string AIChapterSections);

        Task<List<ChapterNonFiction>> BindChapterOutlinesAsync(string BookContentID);

        Task<List<SectionNonFiction>> BindSectionOutlinesAsync(string BookContentID);

        Task<string> SaveBookIntrodution(string BookContentID, string Description, string UserID);

        Task<BookIntroduction> BindContentData_Async(string BookContentID);

        #endregion ai-content-manager

        #region book-writer

        /// <summary>
        /// Purpose :  Get all content data into key value form
        /// </summary>
        /// <param name="content"></param>
        /// <param name="identifiers"></param>
        /// <returns></returns>
        Dictionary<string, string> ExtractBookData(string content, HashSet<string> identifiers);
        /// <summary>
        /// Purpose :  Get all multiline content data into key value form
        /// </summary>
        /// <param name="content"></param>
        /// <param name="identifiers"></param>
        /// <returns></returns>
        Dictionary<string, string> ExtractBookDataFull(string content, HashSet<string> identifiers);
        /// <summary>
        /// Purpose :  Bind model from Dictionary key value data 
        /// </summary>
        /// <param name="bookData"></param>
        /// <returns></returns>
        BookContent GetBookModelFromDictionary(Dictionary<string, string> bookData);
        /// <summary>
        /// Purpose :  Save/Update model data into database
        /// </summary>
        /// <param name="bookContent"></param>
        /// <param name="UserID"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task<Message> SaveUpdateBookAsync(BookContent bookContent, string UserID, string fileName);

        Task<Message> SaveBluePrintResponseAsync(string BookContentID, string UserID, string response1, string response2, string response3, string response4, string response5, string response6);

        Task<Message> SaveBlueprintContentAsync(string BookContentID, string UserID, BlueprintContent blueprintContent);

        Task<Message> UpdateBlueprintDataAsync(string BookContentID, string BlueprintData, string UserID);

        Task<Message> SaveActChaptersAsync(string ParentID, string ContentTypeID, string ActTitle, string ActTitleWithHtml, string UserID, string type);

        #endregion book-writer

        Task<string> CheckBookContent(string ContentCode);

        Task<string> GetColumnValueAsync(string ColumnName);


    }
}
