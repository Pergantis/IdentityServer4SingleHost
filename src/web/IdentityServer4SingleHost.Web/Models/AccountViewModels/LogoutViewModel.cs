namespace IdentityServer4SingleHost.Web.Models.AccountViewModels
{
    public class LogoutViewModel: LogoutInputModel
    {
        public bool ShowLogoutPrompt { get; set; } = true;
    }
}