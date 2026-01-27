using Microsoft.AspNetCore.Rewrite;

namespace JubileeGPT.Middleware
{
    public static class RewriteOptionsExtensions
    {
        public static RewriteOptions AddRedirectToWww(this RewriteOptions options)
        {
            options.Rules.Add(new RedirectToWwwRule());
            return options;
        }
    }
}
