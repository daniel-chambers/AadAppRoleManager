using System.Threading.Tasks;
using AadAppRoleManager.Web.Modules;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AadAppRoleManager.Web.Services
{
    public class AadAccessTokenRepository : IAadAccessTokenRepository
    {
        public const string GraphResourceId = "https://graph.windows.net";

        private readonly string _userObjectId;
        private readonly IConfigurationSettings _configuration;
        private readonly AuthenticationContext _authContext;

        public AadAccessTokenRepository(string tenantId, string userObjectId, IConfigurationSettings configuration, AuthenticationContextFactory authContextFactory)
        {
            _userObjectId = userObjectId;
            _configuration = configuration;
            _authContext = authContextFactory(tenantId, userObjectId);
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var clientCreds = new ClientCredential(_configuration.AadClientId, _configuration.AadAppKey);
            var result = await _authContext.AcquireTokenSilentAsync(GraphResourceId, clientCreds, new UserIdentifier(_userObjectId, UserIdentifierType.UniqueId));
            return result.AccessToken;
        }
    }
}