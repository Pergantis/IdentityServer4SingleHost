using System.Threading.Tasks;

namespace IdentityServer4SingleHost.Web.IdentityAndAccess.Services
{
    public interface IRedirectService
    {
        string ExtractRedirectUriFromReturnUrl(string url);

        Task<string> ExtractOpenEmailAppRedirectUrl(string url);
    }
}
