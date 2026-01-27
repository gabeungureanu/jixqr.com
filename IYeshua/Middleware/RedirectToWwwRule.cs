using Microsoft.AspNetCore.Rewrite;

namespace JubileeGPT.Middleware
{
    public class RedirectToWwwRule : IRule
    {
        public void ApplyRule(RewriteContext context)
        {
            var request = context.HttpContext.Request;
            var host = request.Host;

            if (!host.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            {
                var newHost = new HostString("www." + host.Host);
                var newUrl = $"{request.Scheme}://{newHost}{request.PathBase}{request.Path}{request.QueryString}";

                var response = context.HttpContext.Response;
                response.StatusCode = StatusCodes.Status301MovedPermanently;
                response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Location] = newUrl;
                context.Result = RuleResult.EndResponse;
            }
        }
    }
}
