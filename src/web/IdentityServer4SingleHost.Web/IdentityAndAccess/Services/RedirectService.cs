using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IdentityServer4.Services;
using IdentityServer4.Stores;

namespace IdentityServer4SingleHost.Web.IdentityAndAccess.Services
{
    public class RedirectService: IRedirectService
    {
        private readonly IClientStore _clientStore;
        private readonly IIdentityServerInteractionService _interactionService;

        public RedirectService(IClientStore clientStore, IIdentityServerInteractionService interactionService)
        {
            _clientStore = clientStore;
            _interactionService = interactionService;
        }

        public string ExtractRedirectUriFromReturnUrl(string url)
        {
            var result = "";
            var decodedUrl = System.Net.WebUtility.HtmlDecode(url);
            var results = Regex.Split(decodedUrl, "redirect_uri=");
            if (results.Length < 2)
                return "";

            result = results[1];

            var splitKey = "";
            if (result.Contains("signin-oidc"))
                splitKey = "signin-oidc";
            else
                splitKey = "scope";

            results = Regex.Split(result, splitKey);
            if (results.Length < 2)
                return "";

            result = results[0];

            return result.Replace("%3A", ":").Replace("%2F", "/").Replace("&", "");
        }

        public async Task<string> ExtractOpenEmailAppRedirectUrl(string url)
        {
            var context = await _interactionService.GetAuthorizationContextAsync(url);

            if (context != null)
            {
                (await _clientStore.FindClientByIdAsync(context.ClientId))
                    .Properties.TryGetValue("open_email_app", out string openEmailAppUri);

                return openEmailAppUri;
            }

            return null;
        }
    }
}
