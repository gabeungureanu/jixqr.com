using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.BLImplementation.AccountService
{
    public class ChatGptApiClient
    {
        private const string apiKey = "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"; // Replace with your API key
        private const string apiUrl = "https://api.openai.com/v1/engines/gpt-3.5-turbo/completions";
        //private const string apiUrl = "https://api.openai.com/v1/engines/davinci-codex/completions";

        private readonly HttpClient httpClient;

        //public ChatGptApiClient()
        //{
        //    httpClient = new HttpClient();
        //    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        //}

        //public async Task<string> GetChatResponseAsync(string input)
        //{
        //    try
        //    {
        //        // Prepare the request data
        //        var requestData = new
        //        {
        //            prompt = input,
        //            max_tokens = 50 // Adjust the max_tokens as needed
        //        };

        //        // Serialize the request data to JSON
        //        var requestDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);

        //        // Make an asynchronous API request
        //        var response = await httpClient.PostAsync(apiUrl, new StringContent(requestDataJson, Encoding.UTF8, "application/json"));

        //        // Check if the request was successful
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var responseContent = await response.Content.ReadAsStringAsync();
        //            return responseContent;
        //        }
        //        else
        //        {
        //            // Handle API error here
        //            throw new Exception($"API request failed with status code {response.StatusCode}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}
    }
}
