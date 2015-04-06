using Microsoft.WindowsAzure.Storage.Table;

namespace AadAppRoleManager.Web.Services
{
    public class AadUserTokenCacheEntry : TableEntity
    {
        public byte[] CacheData { get; set; }
    }
}