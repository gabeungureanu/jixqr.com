using DataTypes.ModelDataTypes.Common;
using DataTypes.ModelDataTypes.Home;
using DataTypes.ModelDataTypes.OpenAiAPI;
using DataTypes.ModelDataTypes.Subscription;

namespace BusinessLogic.IBusinessLogic.IOpenAIClientService
{
    public interface IOpenAIApiClientServices
    {
        Message InsertPromptResponse(string Prompt, string Response, string DomainName, string SubPrompt);
        Task<PromptResponse> GetPromptResponseAsync(string domainName, string Prompt, string subPrompt);
        Message UpdatePromptResponse(Guid? ResponseID, string Response);
        string RemovePunctuationMark(string input);
        Message InsertUserPrompt(string UserPrompt, string Prompt, Guid? ResponseID, Guid? UserID, Guid? PromptParentID, Guid? AcoountUserID, Int64 RequestToken, Int64 ResponseToken);
        List<UserParentNodePrompt> GetLoginUserPromptsAsync(Guid? accountUserID, Guid? userID, string parentID, bool IsArchived, bool IsFavorite, bool IsToday);
        Task<PromptResponseReturn> ShowClickedPromptAsync(Guid? Id);
        Task<List<PromptResponseReturn>> ShowClickedSessionPromptAsync(Guid? parentID, bool IsArchived);
        Task<string> SendPromptAsync(string searchText);
        Task<string> SendPromptChatbotAsync(string searchText);
        Task<string> SendPromptUsingChatGptAsync(string userPrompt, string systemPrompt, string disclaimerText, string subPrompt);

        Task<string> SendPromptUsingAzureGptAsync(string searchText, string prefix, string disclaimerText, string subPrompt);
        Task<string> SendPromptUsingAzureGptAsync(string searchText, string prefix);
        Task<string> SendPromptUsingAzureGpt4Async(string userPrompt, string systemPrompt);
        Task<string> SendPromptUsingAzureGpt4ModelAsync(string userPrompt, string systemPrompt, string disclaimerText, string subPrompt);
        Task<string> SendPromptUsingAzureGpt4oModelAsync(string userPrompt, string systemPrompt, string disclaimerText, string subPrompt, bool isLanguage = false);
        string EncodeFormateText(string text, string disclaimerText, bool isLanguage = false);
        Task<string> SendPromptUsingAzureGptAsync(string PersonaContext, string PersonaRole, string NarrativeStructure, string PlotDevelopment, string ThematicDepth, string BlueprintRating);
        Task<int> UpdatePromptDataAsync(Guid? Id, string Prompt);
        Task<int> UpdateParentPromptAsync(Guid? Id, string Prompt);
        Message CreatePromptSession(Guid? UserID, Guid? AccountUserID, string searchText);
        Message GetPromptSessionForToday(Guid? UserID);
        Message DeleteUserPromptAsync(string PromptId, bool isParent, string SelSection);
        Message FavoriteUserPromptAsync(string PromptId, bool isParent);
        Message UnFavoriteUserPromptAsync(string PromptId, bool isParent);
        Task<string> SendPromptUsingAzureGptAsyncAPI(string searchText, string prefix);
        Task<bool> TrackRequest(string UserID);
        Task<List<ModePrompt>> BindModeAsync(string DomainName);
        Task<string> SendMessageAsync(string message);

        Task<PromptResponseReturn> CreateShareChatID(Guid? parentID);
        Task<List<PromptResponseReturn>> ShareChat(Guid? shareID);

        List<UserParentNodePrompt> GetLoginUserPromptsAsyncbyShareID(Guid? shareID);
        string InsertInitialPromptResponse(Guid AccountUserID, Guid UserID, string JsonPromptResponse, Int64 ConsumedToken);
        UserInitialPrompt GetInitialPromptResponse(Guid AccountUserID, Guid UserID);
        List<FeedbackReasonModel> GetAllFeedbackReasons();
    }
}
