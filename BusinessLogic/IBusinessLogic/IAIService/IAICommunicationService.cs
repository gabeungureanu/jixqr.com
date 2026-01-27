using Azure.AI.OpenAI;
using DataTypes.ModelDataTypes.Home;
using DataTypes.ModelDataTypes.Subscription;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.IBusinessLogic.IAIService
{
    public interface IAICommunicationService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemPrompt"></param>
        /// <param name="assistantPrompt"></param>
        /// <param name="userPrompt"></param>
        /// <returns></returns>
        //Task<string> QueryAzureOpenAIAsync(string systemPrompt, string assistantPrompt, string userPrompt);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemPrompt"></param>
        /// <param name="assistantPrompt"></param>
        /// <param name="userPrompt"></param>
        /// <returns></returns>
        //Task<string> QueryChatGptOpenAIAsync(string systemPrompt, string assistantPrompt, string userPrompt);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="userID"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        //Task<DataTypes.ModelDataTypes.Common.Message> CallOpenAIApiAsync(string prompt, string userID, string userName, bool isInitialization);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="input"></param>
        /// <param name="rootPath"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        //Task<DataTypes.ModelDataTypes.Common.Message> GenerateSpeechAsync(string apiKey, string input, string rootPath, string userID, string userName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageHistory"></param>
        /// <param name="userPrompt"></param>
        /// <returns></returns>
        //Task<string> QueryAzureOpenAIWithChatHistorAsync(List<ChatMessage> messageHistory, string userPrompt);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        Task<TextToSpeachVoice> GetVoiceByUserID(string userID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        Task<TextToSpeachVoice> GetHomeVoiceByUserID(string userID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="voiceID"></param>
        /// <returns></returns>
        //Task<DataTypes.ModelDataTypes.Common.Message> SetVoiceByUserIDAsync(string userID, string voiceID);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<List<TextToSpeachVoice>> GetVoice();

        #region New Page
        Task<DataTypes.ModelDataTypes.Common.Message> GenerateSpeechMyPromptAsync(string apiKey, string userInput, string rootPath, string userID, string userName);

        #endregion New Page

        #region New Code

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="conversationSession"></param>
        /// <returns></returns>
        //Task<string> FetchOpenAIApiAsync(string prompt, List<object> conversationSession);
        Task<(string, Int64, Int64)> FetchOpenAIApiAsync(string prompt, List<object> conversationSession, string type = "thread");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conversationSession"></param>
        /// <param name="ConversationCacheKey"></param>
        void AddConversationSessionToCache(List<object> conversationSession, string ConversationCacheKey, IMemoryCache memoryCache);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="systemPrompt"></param>
        /// <param name="conversationCacheKey"></param>
        /// <param name="conversationLength"></param>
        /// <returns></returns>
        List<object> FetchConversationSessionFromCache(string userName, string systemPrompt, string conversationCacheKey, int conversationLength, IMemoryCache memoryCache);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="systemPrompt"></param>
        /// <param name="conversationCacheKey"></param>
        /// <param name="conversationLength"></param>
        /// <param name="memoryCache"></param>
        /// <returns></returns>
        List<object> FetchConversationSessionFromCacheHavingFirstEight(string userName, string systemPrompt, string conversationCacheKey, int conversationLength, IMemoryCache memoryCache);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conversationCacheKey"></param>
        /// <param name="memoryCache"></param>
        /// <returns></returns>
        List<object> FetchOnlyHavingFirstEightConversationFromCache(string conversationCacheKey, IMemoryCache memoryCache);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memoryCache"></param>
        /// <param name="conversationCacheKey"></param>
        void RemoveCacheValue(IMemoryCache memoryCache, string conversationCacheKey);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="disclaimerText"></param>
        /// <returns></returns>
        string EncodeFormateText(string text, string disclaimerText);

        /// <summary>
        /// This will Generate audio file with the name of audio-userid otherwise throw exception
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="rootPath"></param>
        /// <param name="text"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        Task<string> GenerateAudioToUserAsync(string apiKey, string rootPath, string text, string userID);

        #endregion New Code

        /// <summary>
        /// Append Conversation
        /// </summary>
        /// <param name="conversationSession"></param>
        /// <param name="userMemeory"></param>
        /// <param name="conversationCacheKey"></param>
        /// <param name="memoryCache"></param>
        //List<object> AppendConversationWithUserMemeory(List<object> conversationSession, List<object> userMemeory);

        /// <summary>
        /// memory
        /// </summary>
        /// <param name="prompt"></param>
        //void AddUserMemory(Guid AccountUserID,Guid userID, string input, string rootPath, string domainName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conversationCacheKey"></param>
        /// <param name="memoryCache"></param>
        /// <returns></returns>
        //List<object> FetchUserMemoryFromCache(string conversationCacheKey, IMemoryCache memoryCache);
        //List<object> GetCacheValue(string conversationCacheKey, IMemoryCache memoryCache);
        //Task<OpenAIBalance> GetOpenAIBalance();
        //void AddPromptTokenSessionToCache(Int64 promptToken, string conversationCacheKey, IMemoryCache memoryCache);
        //Int64 GetCacheValueForToken(string tokenCacheKey, IMemoryCache memoryCache);
    }
}
