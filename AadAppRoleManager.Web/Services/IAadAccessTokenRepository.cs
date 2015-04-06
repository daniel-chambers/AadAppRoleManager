using System.Threading.Tasks;

namespace AadAppRoleManager.Web.Services
{
    public interface IAadAccessTokenRepository
    {
        Task<string> GetAccessTokenAsync();
    }
}