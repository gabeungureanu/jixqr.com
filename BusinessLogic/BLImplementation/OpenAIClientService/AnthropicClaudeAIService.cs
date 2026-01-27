using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Azure;
using BusinessLogic.IBusinessLogic.IOpenAIClientService;
using Dapper;
using DataAccess;
using DataTypes.ModelDataTypes.Common;
using DataTypes.ModelDataTypes.ContentManager;
using DataTypes.ModelDataTypes.Home;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BusinessLogic.BLImplementation.OpenAIClientService
{
    public class AnthropicClaudeAIService : IAnthropicClaudeAIService
    {
        private readonly IConfiguration _configuration;
        private readonly string _apiKeyClaude;
        private readonly string _modelClaude;
        private readonly DbFactory _dbFactory;
        private readonly HttpClient _httpClient;

        public AnthropicClaudeAIService(IConfiguration configuration, DbFactory dbFactory, IHttpClientFactory httpClientFactory)
        {
            _dbFactory = dbFactory;
            _configuration = configuration;
            _apiKeyClaude = _configuration["ClaudeGPT:APIKey1"] ?? "";
            _modelClaude = _configuration["ClaudeGPT:Model"] ?? "";
            _httpClient = httpClientFactory.CreateClient("AnthropicClient");
        }

        /// <summary>
        /// Purpose         :       Talk with Claude.ai API and get response from it.
        /// Created By      :       Sandeep Aggarwal
        /// Created Date    :       15.March.2024
        /// </summary>
        /// <param name="systemPrompt"></param>
        /// <param name="userPrompt"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AnthropicClaudeResponse> SendPromptUsingAnthropicClaudeAIAsync(string systemPrompt, string userPrompt, string SessionID, string UserID)
        {
            string result = string.Empty;
           // systemPrompt = _configuration["SearchPrefixText:Prefix"] ?? "";
            AnthropicClaudeResponse anthropicClaudeResponse = new AnthropicClaudeResponse();
            try
            {
                var msg = new List<Anthropic.SDK.Messaging.Message>();
                List<UserConversation> userConversations = new List<UserConversation>();
                ///Get session values from database to maintain session
                if (!string.IsNullOrEmpty(SessionID))
                {
                    userConversations = await GetUserConversationUsingSessionID(SessionID);
                }
                if (userConversations.Any())
                {
                    foreach (var conversation in userConversations)
                    {
                        msg.Add(new Anthropic.SDK.Messaging.Message { Role = RoleType.User, Content = FormatHtmlToText(conversation.Prompt ?? "") });
                        msg.Add(new Anthropic.SDK.Messaging.Message { Role = RoleType.Assistant, Content = FormatHtmlToText(conversation.Response ?? "") });
                    }
                }
                msg.Add(new Anthropic.SDK.Messaging.Message { Role = RoleType.User, Content = userPrompt });

                var client = new AnthropicClient(_apiKeyClaude);     
                //client.Messages.
                var messaging = new Anthropic.SDK.Messaging.MessageParameters()
                {
                    SystemMessage = systemPrompt,
                    Messages = msg,
                    Model = _modelClaude,
                    MaxTokens = 2000,
                    Temperature = 0,
                };

                //CancellationToken ctx=new CancellationToken();
                using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(100)))
                {
                    var response = await client.Messages.GetClaudeMessageAsync(messaging, cts.Token);

                    if (response != null)
                    {
                        if (response.Content.Count > 0)
                        {
                            anthropicClaudeResponse.response = response.Content[0].Text;
                            ///Set session values into database on behalf of sessionID
                            if (!string.IsNullOrEmpty(SessionID))
                            {
                                var status = await SetUserConversation(SessionID, userPrompt, FormatTextToHtml(anthropicClaudeResponse.response), UserID);
                            }
                        }
                        if (response.Usage != null)
                        {
                            anthropicClaudeResponse.inputToken = response.Usage.InputTokens;
                            anthropicClaudeResponse.outputToken = response.Usage.OutputTokens;
                        }
                    }
                    anthropicClaudeResponse.sessionID = SessionID;
                }
                return anthropicClaudeResponse;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Purpose         :       Talk with Claude.ai API and get response from it.
        /// Created By      :       Sandeep Aggarwal
        /// Created Date    :       15.March.2024
        /// </summary>
        /// <param name="systemPrompt"></param>
        /// <param name="userPrompt"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AnthropicClaudeResponse> SendPromptUsingAnthropicClaudeAIClientAsync(string systemPrompt, string userPrompt, string SessionID, string UserID)
        {

            string result = string.Empty;
            systemPrompt = _configuration["SearchPrefixText:Prefix"] ?? "";
            AnthropicClaudeResponse anthropicClaudeResponse = new AnthropicClaudeResponse();
            try
            {
                var msg = new List<Anthropic.SDK.Messaging.Message>();
                List<UserConversation> userConversations = new List<UserConversation>();
                ///Get session values from database to maintain session
                if (!string.IsNullOrEmpty(SessionID))
                {
                    userConversations = await GetUserConversationUsingSessionID(SessionID);
                }
                if (userConversations.Any())
                {
                    foreach (var conversation in userConversations)
                    {
                        msg.Add(new Anthropic.SDK.Messaging.Message { Role = RoleType.User, Content = FormatHtmlToText(conversation.Prompt ?? "") });
                        msg.Add(new Anthropic.SDK.Messaging.Message { Role = RoleType.Assistant, Content = FormatHtmlToText(conversation.Response ?? "") });
                    }
                }
                msg.Add(new Anthropic.SDK.Messaging.Message { Role = RoleType.User, Content = "Hello world" });


                // Prepare request parameters
                var messaging = new Anthropic.SDK.Messaging.MessageParameters()
                {
                    SystemMessage = systemPrompt,
                    Messages = msg,
                    Model = _modelClaude,
                    MaxTokens = 2000,
                    Temperature = 0,
                };

                // Create CancellationTokenSource with a timeout
                using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10)))
                {
                    try
                    {
                        // Serialize request parameters to JSON
                        var jsonBody = JsonConvert.SerializeObject(messaging);
                        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                        // Send POST request to Anthropic API
                        var response = await _httpClient.PostAsync("complete", content, cts.Token);

                        // Check if request was successful
                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            // Process the response content
                        }
                        else
                        {
                            // Handle unsuccessful response
                            // You might want to log or handle the error here
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Handle request timeout
                        // You might want to log or handle the timeout here
                    }
                    catch (Exception ex)
                    {
                        // Handle other exceptions
                        // You might want to log or handle the exception here
                    }
                }
            }
            catch (Exception) { throw; }


            return anthropicClaudeResponse;
        }

        /// <summary>
        /// Purpose         :       Get inserted conversation using SessionID
        /// Created By      :       Sandeep Aggarwal
        /// Created Date    :       16.March.2024
        /// </summary>
        /// <param name="SessionID"></param>
        /// <returns></returns>
        private async Task<List<UserConversation>> GetUserConversationUsingSessionID(string SessionID)
        {
            List<UserConversation> userConversations = new List<UserConversation>();
            DynamicParameters param = new DynamicParameters();
            param.Add("@SessionID", SessionID);
            userConversations = await _dbFactory.SelectCommand_SP_List_Async<UserConversation>(userConversations, "gpt_ConversationLogs_GetBySessionID", param);
            return userConversations;
        }

        /// <summary>
        /// Purpose         :       Set conversation using SessionID
        /// Created By      :       Sandeep Aggarwal
        /// Created Date    :       16.March.2024
        /// </summary>
        /// <param name="SessionID"></param>
        /// <param name="Prompt"></param>
        /// <param name="Response"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        private async Task<string> SetUserConversation(string SessionID, string Prompt, string Response, string UserID)
        {
            string Status = string.Empty;
            DynamicParameters param = new DynamicParameters();
            param.Add("@SessionID", SessionID);
            param.Add("@Prompt", Prompt);
            param.Add("@Response", Response);
            param.Add("@UserID", UserID);
            Status = await _dbFactory.InsertCommand_SPQueryAsync(Status, "gpt_ConversationLogs_Insert", param);
            return Status;
        }

        /// <summary>
        /// Purpose         :       Replace string symbols with proper html tags
        /// Created By      :       Sandeep Aggarwal
        /// Created Date    :       16.March.2024
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string FormatTextToHtml(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }
            // Add paragraph breaks for consecutive newlines
            text = Regex.Replace(text, @"(\r\n?|\n)+", "<br>");
            // Add <b> tags for text enclosed in asterisks (e.g., bold)
            text = Regex.Replace(text, @"\*\*(.*?)\*\*", "<b>$1</b>");
            text = Regex.Replace(text, @"\*(.*?)\*", "<b>$1</b>");
            return text;
        }

        /// Purpose         :       Replace html tags with string symbols
        /// Created By      :       Sandeep Aggarwal
        /// Created Date    :       16.March.2024
        public string FormatHtmlToText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }
            // Add paragraph breaks for consecutive newlines
            text = Regex.Replace(text, @"</br>", "\n\n");
            // Add <b> tags for text enclosed in asterisks (e.g., bold)
            text = Regex.Replace(text, @"<b>$1</b>", "\\*");
            return text;
        }
    }
}
