using Anthropic.SDK.Messaging;
using Azure;
using Azure.AI.OpenAI;
using BusinessLogic.IBusinessLogic.IAccountService;
using BusinessLogic.IBusinessLogic.IAIService;
using Dapper;
using DataAccess;
using DataTypes.ModelDataTypes.Common;
using DataTypes.ModelDataTypes.Home;
using DataTypes.ModelDataTypes.Subscription;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BusinessLogic.BLImplementation.AIService
{
    public class AICommunicationService : IAICommunicationService
    {
        private readonly ILogger<AICommunicationService> _logger;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _memoryCache;
        private DbFactory _dbFactory;
        private List<object> _conversationSession = new List<object>();
        private string ConversationCacheKey = "ConversationSession";
        private IAccountService _accountService;
        private const int MaxRetries = 5;
        private const int RetryDelay = 9; // seconds
        /// <summary>
        /// Initialize constructor and inject dependency IConfiguration service
        /// </summary>
        /// <param name="configuration"></param>
        public AICommunicationService(ILogger<AICommunicationService> logger, DbFactory dbFactory, IConfiguration configuration, IMemoryCache memoryCache, IHttpContextAccessor httpContext, IAccountService accountService)
        {
            _config = configuration;
            _logger = logger;
            _memoryCache = memoryCache;
            _dbFactory = dbFactory;
            _accountService = accountService;
            //_conversationSession.Add(new { role = "system", content = _config["SearchPrefixText:Prefix"] ?? "" });
        }

        /// <summary>
        /// Azure OpenAI API endpoint to get response
        /// </summary>
        /// <param name="systemPrompt"></param>
        /// <param name="assistantPrompt"></param>
        /// <param name="userPrompt"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        //public async Task<string> QueryAzureOpenAIAsync(string systemPrompt, string assistantPrompt, string userPrompt)
        //{
        //    //Retrieve configuration values
        //    string? apiEndpoint = _config["AzureGPT4:Endpoint"];
        //    string? apiKey = _config["AzureGPT4:APIKey1"];
        //    string? modelName = _config["AzureGPT4:Deployment4o"];
        //    string defaultUpendPrompt = string.Empty;
        //    // Validate configuration values
        //    if (string.IsNullOrWhiteSpace(apiEndpoint) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(modelName))
        //    {
        //        throw new ArgumentException("Configuration error: API endpoint, key, or model name is missing.");
        //    }

        //    try
        //    {// Token estimation (e.g., use external libraries or manual approximations)
        //        int inputTokenCount = EstimateTokenCount(userPrompt);
        //        int availableTokensForResponse = 4096 - inputTokenCount;

        //        // Ensure a minimum of 50 tokens for a valid response
        //        availableTokensForResponse = Math.Max(availableTokensForResponse, 50);

        //        string conciseInstruction = string.Empty;
        //        if (!string.IsNullOrEmpty(userPrompt))
        //        {
        //            userPrompt = AddConciseInstruction(userPrompt);
        //        }
        //        // Initialize the OpenAI client
        //        var client = new OpenAIClient(new Uri(apiEndpoint), new AzureKeyCredential(apiKey));

        //        // Prepare chat completion options
        //        var chatCompletionsOptions = new ChatCompletionsOptions
        //        {
        //            Messages =
        //                    {
        //                        new ChatMessage(ChatRole.System, systemPrompt ?? string.Empty),
        //                        new ChatMessage(ChatRole.Assistant, assistantPrompt ?? string.Empty),
        //                        new ChatMessage(ChatRole.User, userPrompt ?? string.Empty),
        //                    },
        //            Temperature = 0.3f,//0.75f,
        //            MaxTokens = 4096,
        //            NucleusSamplingFactor = 0.75f,
        //            FrequencyPenalty = 0,
        //            PresencePenalty = 0
        //        };
        //        /// Call Open ai client api to get chat response
        //        Response<ChatCompletions> response = await client.GetChatCompletionsAsync(
        //                                                        modelName,
        //                                                        chatCompletionsOptions);
        //        // Validate API response
        //        if (response.GetRawResponse().Status != 200)
        //        {
        //            throw new HttpRequestException($"API responded with status code {response.GetRawResponse().Status}.");
        //        }
        //        var result = response.Value;
        //        return result?.Choices.FirstOrDefault()?.Message?.Content ?? string.Empty;
        //    }
        //    catch (RequestFailedException ex)
        //    {
        //        // Log and rethrow detailed Azure-specific exceptions
        //        _logger.LogError(ex, "Azure OpenAI API request failed: {Message}", ex.Message);
        //        throw new InvalidOperationException($"Azure OpenAI API request failed with status {ex.Status}: {ex.Message}");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log and rethrow other exceptions
        //        _logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);
        //        throw new InvalidOperationException("An unexpected error occurred while processing the request.", ex);
        //    }
        //}

        /// <summary>
        /// To controle the response from the API use AddConciseInstruction function by passing the user prompt into and get new 
        /// user prompt
        /// </summary>
        /// <param name="userPrompt"></param>
        /// <returns></returns>
        private string AddConciseInstruction(string userPrompt)
        {
            string conciseInstruction = string.Empty;
            try
            {
                if (userPrompt.Contains("brief", StringComparison.OrdinalIgnoreCase) ||
                    userPrompt.Contains("short", StringComparison.OrdinalIgnoreCase) ||
                    userPrompt.Contains("summary", StringComparison.OrdinalIgnoreCase))
                {
                    conciseInstruction = ". Provide the response in 50 words or fewer.";
                }
                else if (userPrompt.Contains("detailed", StringComparison.OrdinalIgnoreCase) ||
                         userPrompt.Contains("detail", StringComparison.OrdinalIgnoreCase) ||
                         userPrompt.Contains("details", StringComparison.OrdinalIgnoreCase) ||
                         userPrompt.Contains("elaborate", StringComparison.OrdinalIgnoreCase))
                {
                    conciseInstruction = ". Provide the response in 200 words or fewer.";
                }
                else if (userPrompt.Contains("explain", StringComparison.OrdinalIgnoreCase) &&
                         userPrompt.Contains("clearly", StringComparison.OrdinalIgnoreCase))
                {
                    conciseInstruction = ". Provide the response in 150 words or fewer.";
                }
                else if (userPrompt.Contains("summarize", StringComparison.OrdinalIgnoreCase) ||
                         userPrompt.Contains("overview", StringComparison.OrdinalIgnoreCase))
                {
                    conciseInstruction = "Provide the response in 100 words or fewer.";
                }
                else if (userPrompt.Contains("step-by-step", StringComparison.OrdinalIgnoreCase) ||
                         userPrompt.Contains("list", StringComparison.OrdinalIgnoreCase) ||
                         userPrompt.Contains("bullet points", StringComparison.OrdinalIgnoreCase))
                {
                    conciseInstruction = ". Provide the response as bullet points, each under 10 words.";
                }
                else if (userPrompt.Contains("in one sentence", StringComparison.OrdinalIgnoreCase) ||
                         userPrompt.Contains("one line", StringComparison.OrdinalIgnoreCase))
                {
                    conciseInstruction = ". Provide the response in a single sentence.";
                }
                else
                {
                    conciseInstruction = ". Provide the response in 100 words or fewer.";
                }
            }
            catch { }
            userPrompt = $"{userPrompt} {conciseInstruction}";
            return userPrompt;
        }

        // Example function to estimate token count (you can replace it with an accurate library)

        /// <summary>
        /// To Calculate token usages into API calling by passing user prompt and return total token to use
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        private int EstimateTokenCount(string messages)
        {
            if (!string.IsNullOrEmpty(messages))
            {
                const int averageTokensPerWord = 1; // Approximation: refine this based on the input type
                return messages.Split(' ').Length * averageTokensPerWord;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// ChatGPT endpoint to get response
        /// </summary>
        /// <param name="systemPrompt"></param>
        /// <param name="assistantPrompt"></param>
        /// <param name="userPrompt"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        //public async Task<string> QueryChatGptOpenAIAsync(string systemPrompt, string assistantPrompt, string userPrompt)
        //{
        //    string API_Endpoint = _config["ChatGpt:API_Endpoint"] ?? "";
        //    string API_Model = _config["ChatGpt:API_Model"] ?? "";
        //    string API_Key = _config["ChatGpt:APIKey"] ?? "";
        //    int API_MaxToken = Convert.ToInt32(_config["ChatGpt:API_MaxToken"] ?? "0");
        //    if (API_MaxToken <= 0) API_MaxToken = 1500;
        //    double API_Temperature = Convert.ToDouble(_config["ChatGpt:API_Temperature"] ?? "0");
        //    if (API_Temperature <= 0) API_Temperature = 0.5;

        //    if (string.IsNullOrEmpty(API_Endpoint) || string.IsNullOrEmpty(API_Model) || string.IsNullOrEmpty(API_Key))
        //    {
        //        throw new Exception("OpenAI API credentials not found!");
        //    }

        //    try
        //    {
        //        //Assign values to payload
        //        var payload = new
        //        {
        //            model = API_Model,//“o” for “omini”
        //            max_tokens = API_MaxToken,
        //            temperature = API_Temperature,
        //            messages = new object[]
        //            {
        //                new { role = "system", content = systemPrompt ?? "" },
        //                new { role = "assistant", content = assistantPrompt ?? "" },
        //                new { role = "user", content =  userPrompt }
        //            }
        //        };
        //        //Instantiate client object
        //        using (var client = new HttpClient())
        //        {
        //            //Allocate Header and pass api key
        //            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_Key}");
        //            client.DefaultRequestHeaders.Add("User-Agent", "OpenAI-CSharp-Client");
        //            //Call GPT API and get response
        //            var response = await client.PostAsync(
        //                API_Endpoint,
        //                new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
        //                );
        //            //Check response is success or not
        //            if (response.IsSuccessStatusCode)
        //            {
        //                var jsonResponse = await response.Content.ReadAsStringAsync();
        //                var responseObject = JObject.Parse(jsonResponse);
        //                var botMessage = responseObject["choices"]?[0]?["message"]?["content"]?.ToString();
        //                //If Chat not found then through an exception
        //                if (botMessage == null)
        //                {
        //                    throw new Exception("Chat response not found");
        //                }
        //                //Return resposne
        //                return botMessage;
        //            }
        //            else
        //            {
        //                throw new Exception($"Error: {response.StatusCode} - {response.ReasonPhrase}");
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //Jai's Code here
        /// <summary>
        /// Get response by passing user prompt into the method perameter and also maintain session upto 10 prompts
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        //public async Task<DataTypes.ModelDataTypes.Common.Message> CallOpenAIApiAsync(string prompt, string userID, string userName, bool isInitialization = false)
        //{
        //    DataTypes.ModelDataTypes.Common.Message message = new DataTypes.ModelDataTypes.Common.Message();
        //    try
        //    {
        //        string API_Endpoint = _config["ChatGpt:API_Endpoint"] ?? "";
        //        string API_Model = _config["ChatGpt:API_Model"] ?? "";
        //        string API_Key = _config["ChatGpt:APIKey"] ?? "";
        //        int API_MaxToken = Convert.ToInt32(_config["ChatGpt:API_MaxToken"] ?? "0");
        //        if (API_MaxToken <= 0) API_MaxToken = 8192;
        //        double API_Temperature = Convert.ToDouble(_config["ChatGpt:API_Temperature"] ?? "0");
        //        if (API_Temperature <= 0) API_Temperature = 0.5;

        //        //if (!string.IsNullOrEmpty(prompt))
        //        //{
        //        //    prompt = AddConciseInstruction(prompt);
        //        //}
        //        ConversationCacheKey = userID;
        //        // Retrieve or initialize the conversation session
        //        if (isInitialization)
        //        {
        //            RemoveCacheValue(_memoryCache, ConversationCacheKey);
        //        }

        //        int conversionCount = GetConversationFromCacheForThreeExecutedPrompt().Count;
        //        if (conversionCount > 0)
        //        {
        //            _conversationSession = GetConversationFromCacheForThreeExecutedPrompt();
        //        }
        //        // Add new user prompt
        //        _conversationSession.Add(new { role = "user", content = prompt });

        //        //if (conversionCount < 6)
        //        //{
        //        //    // Add new user prompt
        //        //    _conversationSession.Add(new { role = "system", content = prompt });
        //        //}
        //        //else
        //        //{
        //        //    // Add new user prompt
        //        //    _conversationSession.Add(new { role = "user", content = prompt });
        //        //}

        //        using (var client = new HttpClient())
        //        {
        //            var requestContent = new
        //            {
        //                model = API_Model,
        //                messages = _conversationSession,
        //                max_tokens = API_MaxToken,
        //                temperature = API_Temperature
        //            };

        //            var jsonContent = System.Text.Json.JsonSerializer.Serialize(requestContent);
        //            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
        //            {
        //                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        //            };

        //            httpRequest.Headers.Add("Authorization", $"Bearer {API_Key}");
        //            client.DefaultRequestHeaders.Add("User-Agent", "OpenAI-CSharp-Client");
        //            var response = await client.SendAsync(httpRequest);

        //            if (response.IsSuccessStatusCode)
        //            {
        //                var jsonResponse = await response.Content.ReadAsStringAsync();
        //                var responseContent = JObject.Parse(jsonResponse);
        //                var botMessage = responseContent["choices"]?[0]?["message"]?["content"]?.ToString();

        //                // Add the assistant's response to the session
        //                _conversationSession.Add(new { role = "assistant", content = botMessage });
        //                // Save the updated session back to the cache
        //                try
        //                {
        //                    SaveConversationSessionToCache(_conversationSession);
        //                }
        //                catch { }
        //                message.Msg = botMessage ?? string.Empty;
        //                message.Status = "1";
        //            }
        //            else
        //            {
        //                message.Msg = "Error in response!";
        //                message.Status = "0";
        //            }
        //        }
        //        return message;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public void RemoveCacheValue(IMemoryCache memoryCache, string conversationCacheKey)
        {
            try
            {
                memoryCache.Remove(conversationCacheKey); // Remove the cached item with the specified key
            }
            catch { }
        }

        /// <summary>
        /// Helper method to retrieve conversation session from cache
        /// </summary>
        /// <returns></returns>
        private List<object> GetConversationSessionFromCache(string userName)
        {
            int sessionLength = Int32.Parse(_config["GPTSession:SessionLength"] ?? "20");
            if (_memoryCache.TryGetValue(ConversationCacheKey, out string? serializedSession))
            {
                var session = System.Text.Json.JsonSerializer.Deserialize<List<object>>(serializedSession ?? "");

                if (session != null && session.Count > 0)
                {
                    // Ensure the first object is included and take the last 20 objects
                    var firstObject = session.First();
                    var lastObjects = session.Skip(1).TakeLast(sessionLength).ToList();
                    // Combine the first object with the last 40 objects
                    var result = new List<object> { firstObject };
                    result.AddRange(lastObjects);
                    return result;
                }
            }
            // If the session is null, initialize with the new system object
            return new List<object>
            {
                new { role = "system", content = (_config["SearchPrefixText:Prefix"] ?? "").Replace("[UserName]", userName) }
            };
        }

        private List<object> GetConversationFromCacheForThreeExecutedPrompt()
        {
            int sessionLength = Int32.Parse(_config["GPTSession:SessionLength"] ?? "20");
            if (_memoryCache.TryGetValue(ConversationCacheKey, out string? serializedSession))
            {
                var session = System.Text.Json.JsonSerializer.Deserialize<List<object>>(serializedSession ?? "");

                if (session != null && session.Count > 0)
                {
                    // Ensure the first object is included and take the last 20 objects
                    var firstSixObject = session.Take(6).ToList();
                    var lastObjects = session.Skip(6).TakeLast(sessionLength).ToList();
                    // Combine the first object with the last 40 objects
                    var result = new List<object>();
                    if (firstSixObject.Count > 0)
                    {
                        result.AddRange(firstSixObject);
                    }
                    if (lastObjects.Count > 0)
                    {
                        result.AddRange(lastObjects);
                    }
                    return result;
                }
            }
            // If the session is null, initialize with the new system object
            return new List<object>();
        }

        /// <summary>
        /// Helper method to save conversation session to cache
        /// </summary>
        /// <param name="conversationSession"></param>
        private void SaveConversationSessionToCache(List<object> conversationSession)
        {
            var serializedSession = System.Text.Json.JsonSerializer.Serialize(conversationSession);
            _memoryCache.Set(ConversationCacheKey, serializedSession, TimeSpan.FromHours(1)); // Adjust expiration as needed
        }

        /// <summary>
        /// Generate bytes by OpenAI api for text-to-speech functionality by passing api key and user prompt
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="userInput"></param>
        /// <returns></returns>
        //public async Task<DataTypes.ModelDataTypes.Common.Message> GenerateSpeechAsync(string apiKey, string userInput, string rootPath, string userID, string userName)
        //{
        //    DataTypes.ModelDataTypes.Common.Message message = new DataTypes.ModelDataTypes.Common.Message();
        //    TextToSpeachVoice textToSpeachVoice = new TextToSpeachVoice();
        //    using var httpClient = new HttpClient();
        //    try
        //    {
        //        textToSpeachVoice = await GetVoiceByUserID(userID);
        //    }
        //    catch (Exception)
        //    {
        //    }
        //    //CALL THE OPEN AI FOR ANSWER
        //    try
        //    {
        //        message = await CallOpenAIApiAsync(userInput, userID, userName);
        //        if (message.Status != "1")
        //        {
        //            throw new Exception(message.Msg);
        //        }

        //        // Set headers for the OpenAI request
        //        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        //        // Create the request body
        //        var requestBody = new
        //        {
        //            input = message.Msg,
        //            voice = textToSpeachVoice?.VoiceAltName ?? "alloy",//"alloy",//"shimmer",
        //            model = "tts-1"//"tts-1"
        //        };

        //        // Convert request body to JSON
        //        string jsonBody = System.Text.Json.JsonSerializer.Serialize(requestBody);
        //        // Send POST request to OpenAI API
        //        var response = await httpClient.PostAsync("https://api.openai.com/v1/audio/speech", new StringContent(jsonBody, Encoding.UTF8, "application/json"));
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var byteArray = await response.Content.ReadAsByteArrayAsync(); // Get audio content as a byte array
        //            try
        //            {
        //                var path1 = Path.Combine(rootPath, "wwwroot");
        //                var path2 = Path.Combine(path1, "Audio");
        //                File.WriteAllBytes(Path.Combine(path2, "audio.mp3"), byteArray);
        //                message.Status = "1";
        //            }
        //            catch (Exception)
        //            {
        //                throw;
        //            }
        //            message.Msg = EncodeFormateText(message.Msg, "");
        //        }
        //        else
        //        {
        //            message.Msg = "Error in getting audio, so unable to talk";
        //            message.Status = "0";
        //        }
        //        return message;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public async Task<TextToSpeachVoice> GetVoiceByUserID(string userID)
        {
            TextToSpeachVoice textToSpeachVoice = new TextToSpeachVoice();
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@UserID", userID);
                textToSpeachVoice = await _dbFactory.SelectCommand_SPAsync(textToSpeachVoice, "system_Users_Voice_Get", param);
                return textToSpeachVoice;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<TextToSpeachVoice> GetHomeVoiceByUserID(string userID)
        {
            TextToSpeachVoice textToSpeachVoice = new TextToSpeachVoice();
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@UserID", userID);
                textToSpeachVoice = await _dbFactory.SelectCommand_SPAsync(textToSpeachVoice, "system_Users_Voice_GetHome", param);
                return textToSpeachVoice;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public async Task<DataTypes.ModelDataTypes.Common.Message> SetVoiceByUserIDAsync(string userID, string voiceID)
        //{
        //    DataTypes.ModelDataTypes.Common.Message message = new DataTypes.ModelDataTypes.Common.Message();
        //    try
        //    {
        //        DynamicParameters param = new DynamicParameters();
        //        param.Add("@UserID", userID);
        //        param.Add("@VoiceID", voiceID);
        //        message = await _dbFactory.SelectCommand_SPAsync(message, "system_Users_Voice_Set", param);
        //        return message;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public async Task<List<TextToSpeachVoice>> GetVoice()
        {
            List<TextToSpeachVoice> textToSpeachVoices = new List<TextToSpeachVoice>();
            try
            {
                DynamicParameters param = new DynamicParameters();
                textToSpeachVoices = await _dbFactory.SelectCommand_SP_List_Async(textToSpeachVoices, "system_Voices_Get", param);
                return textToSpeachVoices;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Change some special char into html tags like \n\r, *, # ,`` etc
        /// </summary>
        /// <param name="text"></param>
        /// <param name="disclaimerText"></param>
        /// <returns></returns>
        public string EncodeFormateText(string text, string disclaimerText)
        {
            string findDisclamer = "DISCLAIMER: Jubilee (GPT) is a digital simulation of a Conservative Christian Believer and not an actual living person.";
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            // Add <b> tags for text enclosed in asterisks (e.g., bold)
            // Patern for make text bold
            // Pattern to match lines starting with #, ##, or ### and replace with <p> tags with class attribute

            string patternI1 = @"""(.*?)""";
            text = Regex.Replace(text, patternI1, "<span class=\"quote\">$1</span>");

            string patternBU1 = @"### (.*)";
            text = Regex.Replace(text, patternBU1, match => $"<span class=\"section-title\"> {match.Groups[1].Value} </span>");

            string patternB1 = @"\*{1,3}(.*?)\*{1,3}";
            text = Regex.Replace(text, patternB1, "<span class=\"bold\">$1</span>");

            // Add paragraph breaks for consecutive newlines
            text = Regex.Replace(text, @"(\r\n?|\n)+", "</p><p>");

            // Add <p> tags to the beginning and end of the text
            text = $"<p>{text}</p>";
            //text = $"<p>{text}<div class='dot-cursor'></div></p>";

            string pattern1 = "<p>" + findDisclamer.Trim() + "</p>";
            string pattern2 = "<p>" + findDisclamer.Trim() + " </p>";
            string pattern3 = "<p><span class=\"bold\">" + findDisclamer.Trim() + "</span></p>";
            string pattern4 = "<p>" + disclaimerText.Trim() + "</p>";
            string pattern5 = "<p>" + disclaimerText.Trim() + " </p>";
            string pattern6 = "<p><span class=\"bold\">" + disclaimerText.Trim() + "</span></p>";
            string replacedText = "<p class='disclaimer'>" + disclaimerText.Trim() + "</p>";
            //-------------------If found "Question:" then make it in bold-------------------------
            text = text.Replace("Question:", "<b>Question:</b>");
            text = text.Replace("question:", "<b>Question:</b>");
            text = text.Replace("QUESTION:", "<b>Question:</b>");

            text = text.Replace(pattern1, replacedText);
            text = text.Replace(pattern2, replacedText);
            text = text.Replace(pattern3, replacedText);
            text = text.Replace(pattern4, replacedText);
            text = text.Replace(pattern5, replacedText);
            text = text.Replace(pattern6, replacedText);

            //if (!text.Contains(disclaimerText.Trim()))
            //{
            //    //text += Environment.NewLine + disclaimerText.Trim();
            //    text += "<p class='disclaimer'>" + disclaimerText.Trim() + "</p>";
            //}

            //Contain it into a div
            text = "<div>" + text + "</div>";

            return text;
        }
        //Jai's Code End here

        /// <summary>
        /// Get response from the Azure AI GPT API data with along session maintains by passing user prompt along with chatmessage list.
        /// </summary>
        /// <param name="messageHistory"></param>
        /// <param name="userPrompt"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        //public async Task<string> QueryAzureOpenAIWithChatHistorAsync(List<ChatMessage> messageHistory, string userPrompt)
        //{
        //    //Retrieve configuration values
        //    string? apiEndpoint = _config["AzureGPT4:Endpoint"];
        //    string? apiKey = _config["AzureGPT4:APIKey1"];
        //    string? modelName = _config["AzureGPT4:Deployment4o"];
        //    // Validate configuration values
        //    if (string.IsNullOrWhiteSpace(apiEndpoint) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(modelName))
        //    {
        //        throw new ArgumentException("Configuration error: API endpoint, key, or model name is missing.");
        //    }

        //    try
        //    {
        //        if (string.IsNullOrEmpty(userPrompt))
        //        {
        //            throw new ArgumentException("Missing user prompt.");
        //        }

        //        // Initialize the OpenAI client
        //        var client = new OpenAIClient(new Uri(apiEndpoint), new AzureKeyCredential(apiKey));
        //        // Add user prompt here 
        //        messageHistory.Add(new ChatMessage(ChatRole.User, userPrompt));
        //        // Prepare chat completion options
        //        var chatCompletionsOptions = new ChatCompletionsOptions
        //        {
        //            Temperature = 0.75f,
        //            MaxTokens = 4096,
        //            NucleusSamplingFactor = 0.75f,
        //            FrequencyPenalty = 0,
        //            PresencePenalty = 0
        //        };
        //        // Populate the Messages collection with the history
        //        foreach (var message in messageHistory)
        //        {
        //            chatCompletionsOptions.Messages.Add(message);
        //        }
        //        /// Call Open ai client api to get chat response
        //        Response<ChatCompletions> response = await client.GetChatCompletionsAsync(
        //                                                        modelName,
        //                                                        chatCompletionsOptions);
        //        // Validate API response
        //        if (response.GetRawResponse().Status != 200)
        //        {
        //            throw new HttpRequestException($"API responded with status code {response.GetRawResponse().Status}.");
        //        }
        //        var result = response.Value;
        //        return result?.Choices.FirstOrDefault()?.Message?.Content ?? string.Empty;
        //    }
        //    catch (RequestFailedException ex)
        //    {
        //        // Log and rethrow detailed Azure-specific exceptions
        //        _logger.LogError(ex, "Azure OpenAI API request failed: {Message}", ex.Message);
        //        throw new InvalidOperationException($"Azure OpenAI API request failed with status {ex.Status}: {ex.Message}");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log and rethrow other exceptions
        //        _logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);
        //        throw new InvalidOperationException("An unexpected error occurred while processing the request.", ex);
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string SanitizeText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Remove * and # and trim the string
            return Regex.Replace(text, @"[*#]", "").Trim();
        }

        #region New Page
        public async Task<DataTypes.ModelDataTypes.Common.Message> GenerateSpeechMyPromptAsync(string apiKey, string userInput, string rootPath, string userID, string userName)
        {
            DataTypes.ModelDataTypes.Common.Message message = new DataTypes.ModelDataTypes.Common.Message();
            TextToSpeachVoice textToSpeachVoice = new TextToSpeachVoice();
            using var httpClient = new HttpClient();
            try
            {
                textToSpeachVoice = await GetVoiceByUserID(userID);
            }
            catch (Exception)
            {
            }
            //CALL THE OPEN AI FOR ANSWER
            try
            {
                message = await CallOpenAIApiMyPromptAsync(userInput, userID, userName);
                if (message.Status != "1")
                {
                    throw new Exception(message.Msg);
                }

                // Set headers for the OpenAI request
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                // Create the request body
                var requestBody = new
                {
                    input = message.Msg,
                    voice = textToSpeachVoice?.VoiceAltName ?? "alloy",//"alloy",//"shimmer",
                    model = "tts-1"//"tts-1"
                };

                // Convert request body to JSON
                string jsonBody = System.Text.Json.JsonSerializer.Serialize(requestBody);
                // Send POST request to OpenAI API
                var response = await httpClient.PostAsync("https://api.openai.com/v1/audio/speech", new StringContent(jsonBody, Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    var byteArray = await response.Content.ReadAsByteArrayAsync(); // Get audio content as a byte array
                    try
                    {
                        var path1 = Path.Combine(rootPath, "wwwroot");
                        var path2 = Path.Combine(path1, "Audio");
                        File.WriteAllBytes(Path.Combine(path2, "audioNew.mp3"), byteArray);
                        message.Status = "1";
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    message.Msg = EncodeFormateText(message.Msg, "");
                }
                else
                {
                    message.Msg = "Error in getting audio, so unable to talk";
                    message.Status = "0";
                }
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataTypes.ModelDataTypes.Common.Message> CallOpenAIApiMyPromptAsync(string prompt, string userID, string userName, bool isInitialization = false)
        {
            DataTypes.ModelDataTypes.Common.Message message = new DataTypes.ModelDataTypes.Common.Message();
            try
            {
                string API_Endpoint = _config["ChatGpt:API_Endpoint"] ?? "";
                string API_Model = _config["ChatGpt:API_Model"] ?? "";
                string API_Key = _config["ChatGpt:APIKey"] ?? "";
                int API_MaxToken = Convert.ToInt32(_config["ChatGpt:API_MaxToken"] ?? "0");
                if (API_MaxToken <= 0) API_MaxToken = 8192;
                double API_Temperature = Convert.ToDouble(_config["ChatGpt:API_Temperature"] ?? "0");
                if (API_Temperature <= 0) API_Temperature = 0.5;

                if (!string.IsNullOrEmpty(prompt))
                {
                    prompt = AddConciseInstruction(prompt);
                }
                ConversationCacheKey = userID + "my-prompt";
                // Retrieve or initialize the conversation session
                if (isInitialization)
                {
                    RemoveCacheValue(_memoryCache, ConversationCacheKey);
                }

                int conversionCount = GetConversationSessionFromCache(userName).Count;
                if (conversionCount > 0)
                {
                    _conversationSession = GetConversationSessionFromCache(userName);
                }
                // Add new user prompt
                _conversationSession.Add(new { role = "user", content = prompt });

                using (var client = new HttpClient())
                {
                    var requestContent = new
                    {
                        model = API_Model,
                        messages = _conversationSession,
                        max_tokens = API_MaxToken,
                        temperature = API_Temperature
                    };

                    var jsonContent = System.Text.Json.JsonSerializer.Serialize(requestContent);
                    var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
                    {
                        Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                    };

                    httpRequest.Headers.Add("Authorization", $"Bearer {API_Key}");
                    client.DefaultRequestHeaders.Add("User-Agent", "OpenAI-CSharp-Client");
                    var response = await client.SendAsync(httpRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var responseContent = JObject.Parse(jsonResponse);
                        var botMessage = responseContent["choices"]?[0]?["message"]?["content"]?.ToString();

                        // Add the assistant's response to the session
                        _conversationSession.Add(new { role = "assistant", content = botMessage });
                        // Save the updated session back to the cache
                        try
                        {
                            SaveConversationSessionToCache(_conversationSession);
                        }
                        catch { }
                        message.Msg = botMessage ?? string.Empty;
                        message.Status = "1";
                    }
                    else
                    {
                        message.Msg = "Error in response!";
                        message.Status = "0";
                    }
                }
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion New Page

        #region New Code for API call

        public List<object> RemoveTypePropertyFromList(List<object> list)
        {
            return list.Select(item =>
            {
                // Dynamically get properties, excluding "type"
                var dict = item.GetType()
                               .GetProperties()
                               .Where(p => p.Name != "type")  // Exclude the "type" property
                               .ToDictionary(p => p.Name, p => p.GetValue(item));

                // Create a new anonymous object without the "type" property
                return new { role = dict["role"], content = dict["content"] } as object;
            }).ToList();
        }

        /// <summary>
        /// Fetch OpenAI Api result
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="conversationSession"></param>
        /// <returns></returns>
        public async Task<(string, Int64, Int64)> FetchOpenAIApiAsync(string prompt, List<object> conversationSession, string type = "thread")
        {
            StringBuilder sb = new StringBuilder();
            Int64 promptTokens = 0;
            Int64 completionTokens = 0;
            List<object> messages = new List<object>();
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    string API_Endpoint = _config["ChatGpt:API_Endpoint"] ?? "";
                    string API_Model = _config["ChatGpt:API_Model"] ?? "";
                    string API_Key = _config["ChatGpt:APIKey"] ?? "";
                    int API_MaxToken = Convert.ToInt32(_config["ChatGpt:API_MaxToken"] ?? "0");
                    if (API_MaxToken <= 0) API_MaxToken = 8192;
                    double API_Temperature = Convert.ToDouble(_config["ChatGpt:API_Temperature"] ?? "0");
                    if (API_Temperature <= 0) API_Temperature = 0.7;
                    // Validate required
                    if (string.IsNullOrEmpty(API_Endpoint) || string.IsNullOrEmpty(API_Key) || string.IsNullOrEmpty(API_Model))
                    {
                        throw new Exception("Error: unable to get API_Endpoint or API_Key or API_Model");
                    }

                    // Add new user prompt
                    //if (!string.IsNullOrEmpty(prompt))
                    //    conversationSession.Add(new { role = "user", content = prompt});

                    if (!string.IsNullOrEmpty(prompt))
                    {
                        conversationSession.Add(new { role = "user", content = prompt, type = type });
                    }

                    messages = RemoveTypePropertyFromList(conversationSession);

                    using (var client = new HttpClient())
                    {
                        var requestContent = new
                        {
                            model = API_Model,
                            messages = conversationSession,
                            max_tokens = API_MaxToken,
                            temperature = API_Temperature
                        };

                        var jsonContent = System.Text.Json.JsonSerializer.Serialize(requestContent);
                        var httpRequest = new HttpRequestMessage(HttpMethod.Post, API_Endpoint)
                        {
                            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                        };

                        httpRequest.Headers.Add("Authorization", $"Bearer {API_Key}");
                        client.DefaultRequestHeaders.Add("User-Agent", "OpenAI-CSharp-Client");
                        var response = await client.SendAsync(httpRequest);


                        if (response.IsSuccessStatusCode)
                        {
                            //throw new Exception($"Error: Fetching error like {response.StatusCode}");
                            var jsonResponse = await response.Content.ReadAsStringAsync();


                            var responseContent = JObject.Parse(jsonResponse);
                            var botMessage = responseContent["choices"]?[0]?["message"]?["content"]?.ToString();
                            sb.Append(botMessage ?? string.Empty);


                            var jsonDoc = JsonDocument.Parse(jsonResponse);
                            promptTokens = jsonDoc.RootElement.GetProperty("usage").GetProperty("prompt_tokens").GetInt32();
                            completionTokens = jsonDoc.RootElement.GetProperty("usage").GetProperty("completion_tokens").GetInt32();


                            return (sb.ToString() ?? string.Empty, promptTokens, completionTokens);
                        }
                        else if ((int)response.StatusCode == 429) // Rate limit error
                        {
                            var retryAfter = RetryDelay;

                            var responseBody = await response.Content.ReadAsStringAsync();
                            var match = Regex.Match(responseBody, @"Please try again in (\d+\.\d+)s");

                            if (match.Success && double.TryParse(match.Groups[1].Value, out var retrySeconds))
                            {
                                retryAfter = (int)Math.Ceiling(retrySeconds);
                            }
                            else
                            {
                                retryAfter *= (int)Math.Pow(2, attempt);
                            }

                            await Task.Delay(retryAfter * 1000);
                        }
                        else
                        {
                            var errorMessage = await response.Content.ReadAsStringAsync();
                            var errorJson = JObject.Parse(errorMessage);
                            if (errorJson["error"]?["code"]?.ToString() == "context_length_exceeded")
                            {
                                throw new InvalidOperationException($"Token limit exceeded: {errorJson["error"]?["message"]}");
                            }
                            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                throw new UnauthorizedAccessException($"Unauthorized: {errorMessage}");
                            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                                throw new Exception($"Rate limit exceeded: {errorMessage}");

                        }
                    }
                    return (sb.ToString(), promptTokens, completionTokens);
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Network error: {ex.Message}");
                    throw;
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Invalid API key. Please check your credentials.");
                    throw;
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Request timed out. Try again later.");
                    throw;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return ("fail", 0, 0);
        }

        /// <summary>
        /// Fetch OpenAI Api result
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="conversationSession"></param>
        /// <returns></returns>
        //public void AddUserMemory(Guid AccountUserID, Guid userID, string input, string rootPath, string domainName)
        //{
        //    Task.Run(async () =>
        //    {
        //        var content = await ReadFiles("askJubilee_userMemory_step5.gpt", rootPath);
        //        if (!string.IsNullOrEmpty(content))
        //        {
        //            var prompt = content.Replace("CONTENT", input);
        //            StringBuilder sb = new StringBuilder();
        //            for (int attempt = 0; attempt < MaxRetries; attempt++)
        //            {
        //                try
        //                {
        //                    string API_Endpoint = _config["ChatGpt:API_Endpoint"] ?? "";
        //                    string API_Model = _config["ChatGpt:API_Model"] ?? "";
        //                    string API_Key = _config["ChatGpt:APIKey"] ?? "";
        //                    int API_MaxToken = Convert.ToInt32(_config["ChatGpt:API_MaxToken"] ?? "0");
        //                    if (API_MaxToken <= 0) API_MaxToken = 8192;
        //                    double API_Temperature = 0.2;

        //                    // Validate required
        //                    var conversationSession = new object[]
        //                    {
        //                        new
        //                        {
        //                            role = "user",
        //                            content = prompt.Trim()
        //                        }
        //                    };

        //                    using (var client = new HttpClient())
        //                    {
        //                        var requestContent = new
        //                        {
        //                            model = API_Model,
        //                            messages = conversationSession,
        //                            max_tokens = API_MaxToken,
        //                            temperature = API_Temperature
        //                        };

        //                        var jsonContent = System.Text.Json.JsonSerializer.Serialize(requestContent);
        //                        var httpRequest = new HttpRequestMessage(HttpMethod.Post, API_Endpoint)
        //                        {
        //                            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        //                        };

        //                        httpRequest.Headers.Add("Authorization", $"Bearer {API_Key}");
        //                        client.DefaultRequestHeaders.Add("User-Agent", "OpenAI-CSharp-Client");
        //                        var response = await client.SendAsync(httpRequest);

        //                        if (response.IsSuccessStatusCode)
        //                        {
        //                            //throw new Exception($"Error: Fetching error like {response.StatusCode}");
        //                            var jsonResponse = await response.Content.ReadAsStringAsync();
        //                            var responseContent = JObject.Parse(jsonResponse);
        //                            var botMessage = responseContent["choices"]?[0]?["message"]?["content"]?.ToString();
        //                            if (botMessage is not null && botMessage.ToLower() != "none")
        //                            {
        //                                _accountService.AddUserMemory(AccountUserID, userID, botMessage, domainName);
        //                                //Update User Memory into Cache again                                                                         
        //                                var userMemory = _accountService.GetUserMemory(AccountUserID, userID, domainName == null ? "localhost" : domainName);
        //                                if (userMemory != null && userMemory.Count > 0)
        //                                {
        //                                    string memoryKey = userID + "-memory";
        //                                    RemoveCacheValue(_memoryCache, memoryKey);
        //                                    AddConversationSessionToCache(userMemory, memoryKey, _memoryCache);
        //                                }
        //                            }
        //                            break;
        //                        }
        //                        else if ((int)response.StatusCode == 429) // Rate limit error
        //                        {
        //                            var retryAfter = RetryDelay;

        //                            var responseBody = await response.Content.ReadAsStringAsync();
        //                            var match = Regex.Match(responseBody, @"Please try again in (\d+\.\d+)s");

        //                            if (match.Success && double.TryParse(match.Groups[1].Value, out var retrySeconds))
        //                            {
        //                                retryAfter = (int)Math.Ceiling(retrySeconds);
        //                            }
        //                            else
        //                            {
        //                                retryAfter *= (int)Math.Pow(2, attempt);
        //                            }

        //                            await Task.Delay(retryAfter * 1000);
        //                        }
        //                        else
        //                        {
        //                            var errorMessage = await response.Content.ReadAsStringAsync();
        //                            //throw new Exception($"Error: Fetching error like {response.StatusCode}");
        //                            break;
        //                        }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine(ex.Message.ToString());
        //                    break;
        //                }
        //            }
        //        }
        //    });
        //}

        /// <summary>
        /// Add Conversation Session To Cache
        /// </summary>
        /// <param name="conversationSession"></param>
        /// <param name="conversationCacheKey"></param>
        /// <param name="memoryCache"></param>
        public void AddConversationSessionToCache(List<object> conversationSession, string conversationCacheKey, IMemoryCache memoryCache)
        {
            try
            {
                //var serializedSession = System.Text.Json.JsonSerializer.Serialize(conversationSession);
                memoryCache.Set(conversationCacheKey, conversationSession, TimeSpan.FromHours(24)); // Adjust expiration as needed
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Add Conversation Session To Cache
        /// </summary>
        /// <param name="promptTokenSession"></param>
        /// <param name="tokenCacheKey"></param>
        /// <param name="memoryCache"></param>
        //public void AddPromptTokenSessionToCache(Int64 promptToken, string tokenCacheKey, IMemoryCache memoryCache)
        //{
        //    try
        //    {
        //        //var serializedSession = System.Text.Json.JsonSerializer.Serialize(conversationSession);
        //        memoryCache.Set(tokenCacheKey, promptToken.ToString(), TimeSpan.FromHours(24)); // Adjust expiration as needed
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}


        /// <summary>
        /// Add Conversation Session To Cache
        /// </summary>
        /// <param name="conversationSession"></param>
        /// <param name="conversationCacheKey"></param>
        /// <param name="memoryCache"></param>
        //public List<object> AppendConversationWithUserMemeory(List<object> conversationSession, List<object> userMemeory)
        //{
        //    try
        //    {
        //        //conversationSession.AddRange
        //        var serializedSession = System.Text.Json.JsonSerializer.Serialize(conversationSession);
        //        int insertIndex = 8;
        //        if (insertIndex < conversationSession.Count)
        //        {
        //            conversationSession.InsertRange(insertIndex, userMemeory);
        //        }
        //        else
        //        {
        //            // Append at the end if the index is out of bounds
        //            conversationSession.AddRange(userMemeory);
        //        }
        //        //memoryCache.Set(conversationCacheKey, serializedSession, TimeSpan.FromHours(10));
        //        return conversationSession;
        //    }
        //    catch (Exception)
        //    {
        //        return new List<object>();
        //    }
        //}
        /// <summary>
        /// Fetch Conversation Session From Cache
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="systemPrompt"></param>
        /// <param name="conversationCacheKey"></param>
        /// <param name="conversationLength"></param>
        /// <param name="memoryCache"></param>
        /// <returns></returns>
        public List<object> FetchConversationSessionFromCache(string userName, string systemPrompt, string conversationCacheKey, int conversationLength, IMemoryCache memoryCache)
        {
            try
            {
                if (memoryCache.TryGetValue(conversationCacheKey, out List<object>? session))
                {
                    //var session = System.Text.Json.JsonSerializer.Deserialize<List<object>>(serializedSession ?? "");

                    if (session != null && session.Count > 0)
                    {
                        // Ensure the first object is included and take the last 20 objects
                        var firstObject = session.First();
                        var lastObjects = session.Skip(8).TakeLast(conversationLength).ToList();
                        // Combine the first object with the last 40 objects
                        var result = new List<object> { firstObject };
                        result.AddRange(lastObjects);
                        return result;
                    }
                }
                // If the session is null, initialize with the new system object
                return new List<object>
                {
                    new { role = "system", content = systemPrompt.Replace("[UserName]", userName), type = "default" }
                };
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Fetch Conversation Session From Cache
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="systemPrompt"></param>
        /// <param name="conversationCacheKey"></param>
        /// <param name="conversationLength"></param>
        /// <param name="memoryCache"></param>
        /// <returns></returns>
        //public List<object> GetCacheValue(string conversationCacheKey, IMemoryCache memoryCache)
        //{
        //    try
        //    {
        //        if (memoryCache.TryGetValue(conversationCacheKey, out List<object>? session) && session != null)
        //        {
        //            return session.Take(16).ToList();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //    // Return empty list if cache does not exist
        //    return new List<object>();
        //}

        /// <summary>
        /// Fetch Conversation Session From Cache
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="systemPrompt"></param>
        /// <param name="tokenCacheKey"></param>
        /// <param name="memoryCache"></param>
        /// <returns></returns>
        //public Int64 GetCacheValueForToken(string tokenCacheKey, IMemoryCache memoryCache)
        //{
        //    if (memoryCache.TryGetValue(tokenCacheKey, out object? value))
        //    {
        //        if (value != null)
        //        {
        //            return Convert.ToInt64(value.ToString());
        //        }
        //    }

        //    return 0;
        //}



        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="systemPrompt"></param>
        /// <param name="conversationCacheKey"></param>
        /// <param name="conversationLength"></param>
        /// <param name="memoryCache"></param>
        /// <returns></returns>
        //public List<object> FetchUserMemoryFromCache(string conversationCacheKey, IMemoryCache memoryCache)
        //{
        //    try
        //    {
        //        if (memoryCache.TryGetValue(conversationCacheKey, out List<object>? session))
        //        {
        //            //var session = System.Text.Json.JsonSerializer.Deserialize<List<object>>(serializedSession ?? "");

        //            if (session != null && session.Count > 0)
        //            {
        //                return session;
        //            }
        //        }
        //        return new List<object>();
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="systemPrompt"></param>
        /// <param name="conversationCacheKey"></param>
        /// <param name="conversationLength"></param>
        /// <param name="memoryCache"></param>
        /// <returns></returns>
        public List<object> FetchConversationSessionFromCacheHavingFirstEight(string userName, string systemPrompt, string conversationCacheKey, int conversationLength, IMemoryCache memoryCache)
        {
            try
            {
                if (memoryCache.TryGetValue(conversationCacheKey, out List<object>? session))
                {
                    //var session = System.Text.Json.JsonSerializer.Deserialize<List<object>>(serializedSession ?? "");

                    if (session != null && session.Count > 0)
                    {
                        // Ensure the first object is included and take the last 20 objects
                        //var firstObject = session.First();
                        var firstEightObject = session.Take(16);
                        var lastObjects = session.Skip(16).TakeLast(conversationLength).ToList();
                        // Combine the first object with the last 40 objects
                        //var result = new List<object> { firstObject };
                        var result = new List<object>();
                        result.AddRange(firstEightObject);
                        if (lastObjects != null)
                            result.AddRange(lastObjects);
                        return result;
                    }
                }
                // If the session is null, initialize with the new system object
                return new List<object>
                {
                    new { role = "system", content = systemPrompt.Replace("[UserName]", userName),type = "default" }
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="systemPrompt"></param>
        /// <param name="conversationCacheKey"></param>
        /// <param name="conversationLength"></param>
        /// <param name="memoryCache"></param>
        /// <returns></returns>
        public List<object> FetchOnlyHavingFirstEightConversationFromCache(string conversationCacheKey, IMemoryCache memoryCache)
        {
            try
            {
                if (memoryCache.TryGetValue(conversationCacheKey, out List<object>? session))
                {
                    //var session = System.Text.Json.JsonSerializer.Deserialize<List<object>>(serializedSession ?? "");

                    if (session != null && session.Count > 0)
                    {
                        var firstEightObject = session.Take(16);
                        var result = new List<object>();
                        result.AddRange(firstEightObject);
                        return result;
                    }
                }
                // If the session is null, initialize with the new system object
                return new List<object>();
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// This will Generate audio file with the name of audio-userid otherwise throw exception
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="rootPath"></param>
        /// <param name="text"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public async Task<string> GenerateAudioToUserAsync(string apiKey, string rootPath, string text, string userID)
        {
            string message = string.Empty;
            TextToSpeachVoice textToSpeachVoice = new TextToSpeachVoice();
            using var httpClient = new HttpClient();
            try
            {
                textToSpeachVoice = await GetVoiceByUserID(userID);
            }
            catch (Exception)
            {
            }
            //CALL THE OPEN AI FOR ANSWER
            try
            {
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new Exception("Error: Unable to get api key to generate voice");
                }
                if (string.IsNullOrEmpty(text))
                {
                    throw new Exception("Error: Unable to get text to generate voice");
                }

                // Set headers for the OpenAI request
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                // Create the request body
                var requestBody = new
                {
                    input = text,
                    voice = textToSpeachVoice?.VoiceAltName ?? "nova",//"alloy",//"shimmer",
                    model = "tts-1-hd" //"tts-1"
                };

                // Convert request body to JSON
                string jsonBody = System.Text.Json.JsonSerializer.Serialize(requestBody);
                // Send POST request to OpenAI API
                var response = await httpClient.PostAsync("https://api.openai.com/v1/audio/speech", new StringContent(jsonBody, Encoding.UTF8, "application/json"));
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error: {response.StatusCode} unable to generate audio file");
                }
                var byteArray = await response.Content.ReadAsByteArrayAsync(); // Get audio content as a byte array
                var path1 = Path.Combine(rootPath, "wwwroot");
                var path2 = Path.Combine(path1, "Audio");
                File.WriteAllBytes(Path.Combine(path2, "audio-" + userID + ".mp3"), byteArray);
                message = "Success";
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion New Code for API call

        /// <summary>
        /// Read data from the file and retrun as string
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private async Task<string> ReadFiles(string fileName, string rootPath)
        {
            string content = string.Empty;
            try
            {
                string folderPath = Path.Combine(rootPath, "wwwroot/OpenAIModelReadySteps"); // Replace with your actual folder path
                string filePath = Path.Combine(folderPath, fileName);
                if (Directory.Exists(folderPath) && System.IO.File.Exists(filePath))
                {
                    StreamReader reader = new StreamReader(filePath);
                    content = await reader.ReadToEndAsync();
                }
                return content;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Get Open API balance
        //public async Task<OpenAIBalance> GetOpenAIBalance()
        //{
        //    try
        //    {
        //        string API_Key = _config["ChatGpt:APIKey"] ?? "";
        //        using (HttpClient client = new HttpClient())
        //        {
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_Key);

        //            try
        //            {
        //                HttpResponseMessage response = await client.GetAsync("https://api.openai.com/v1/dashboard/billing/credit_grants");
        //                response.EnsureSuccessStatusCode();

        //                string responseBody = await response.Content.ReadAsStringAsync();
        //                JObject json = JObject.Parse(responseBody);

        //                double totalCredits = json["total_granted"]?.Value<double>() ?? 0;
        //                double usedCredits = json["total_used"]?.Value<double>() ?? 0;
        //                double remainingCredits = json["total_available"]?.Value<double>() ?? 0;

        //                return new OpenAIBalance
        //                {
        //                    TotalCredits = totalCredits,
        //                    UsedCredits = usedCredits,
        //                    RemainingCredits = remainingCredits
        //                };
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine($"Error: {ex.Message}");
        //                return new OpenAIBalance();
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return new OpenAIBalance();
        //        throw;
        //    }
        //}



    }
}
