using Anthropic.SDK;
using Anthropic.SDK.Completions;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BusinessLogic.BLImplementation.OpenAIClientService
{
    public class ChatCompletionsOptionsWithSession : ChatCompletionsOptions
    {
        private IConfiguration _configuration;
        public ChatCompletionsOptionsWithSession(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> SendPromptUsingAnthropicClaudeAsync(string systemPrompt, string userPrompt)
        {
            string result = string.Empty;
            try
            {
                var key = _configuration["ClaudeGPT:APIKey1"];
                var client = new AnthropicClient(key);
                var message = new Anthropic.SDK.Messaging.MessageParameters()
                {
                    SystemMessage = systemPrompt,
                    Messages = new List<Anthropic.SDK.Messaging.Message>
                    {
                        new Anthropic.SDK.Messaging.Message { Role = "user", Content = userPrompt }
                    },
                    Model = "claude-3-opus-20240229",
                    MaxTokens = 100,
                    Temperature = 0,
                };
                var response = await client.Messages.GetClaudeMessageAsync(message);
                if (response != null)
                {
                    if (response.Content.Count > 0)
                    {
                        result = response.Content[0].Text;
                    }
                }
                return result;
                // Handle response
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}");
            }
        }
    }
}
