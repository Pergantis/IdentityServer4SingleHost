using System.Threading.Tasks;

namespace IdentityServer4SingleHost.Web.Extensions
{
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
    }
}
