using DataTypes.ModelDataTypes.ContentManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.IBusinessLogic.IOpenAIClientService
{
    public interface IAnthropicClaudeAIService
    {
        /// <summary>
        /// Purpose         :       Talk with Claude.ai API and get response from it.   
        /// Created By      :       Sandeep Aggarwal
        /// Created Date    :       15.March.2024
        /// </summary>
        /// <param name="systemPrompt"></param>
        /// <param name="userPrompt"></param>
        /// <returns></returns>
        Task<AnthropicClaudeResponse> SendPromptUsingAnthropicClaudeAIAsync(string systemPrompt, string userPrompt, string SessionID,string UserID);

        string FormatTextToHtml(string text);

        string FormatHtmlToText(string text);

        Task<AnthropicClaudeResponse> SendPromptUsingAnthropicClaudeAIClientAsync(string systemPrompt, string userPrompt, string SessionID, string UserID);
    }
}
