using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AadAppRoleManager.Web.Services
{
    public interface IAadAccessTokenRepository
    {
        Task<string> GetAccessTokenAsync();
        Task<AuthenticationResult> GetAuthenticationResultAsync();
    }
}