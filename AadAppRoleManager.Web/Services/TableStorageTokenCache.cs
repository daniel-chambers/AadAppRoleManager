using System.Linq;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AadAppRoleManager.Web.Services
{
    public class TableStorageTokenCache : TokenCache
    {
        private readonly string _tenantId;
        private readonly string _userObjectId;
        private readonly CloudTable _table;
        private AadUserTokenCacheEntry _entry;

        public TableStorageTokenCache(string tenantId, string userObjectId, CloudStorageAccount cloudStorageAccount)
        {
            _tenantId = tenantId;
            _userObjectId = userObjectId;
            var tableClient = cloudStorageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference(TableNames.AadUserTokenCacheEntries);

            BeforeAccess = OnBeforeAccess;
            AfterAccess = OnAfterAccess;
        }

        private void OnBeforeAccess(TokenCacheNotificationArgs args)
        {
            ReadFromTable();
        }

        private void OnAfterAccess(TokenCacheNotificationArgs args)
        {
            if (HasStateChanged)
            {
                WriteToTable();
                HasStateChanged = false;
            }
        }

        private void ReadFromTable()
        {
            // ReSharper disable once ReplaceWithSingleCallToFirstOrDefault
            _entry = _table.CreateQuery<AadUserTokenCacheEntry>()
                .Where(e => e.PartitionKey == _tenantId && e.RowKey == _userObjectId)
                .FirstOrDefault();

            if (_entry != null)
                Deserialize(_entry.CacheData);
        }

        private void WriteToTable()
        {
            if (_entry == null)
            {
                _entry = new AadUserTokenCacheEntry
                {
                    PartitionKey = _tenantId,
                    RowKey = _userObjectId,
                };
            }
            _entry.CacheData = Serialize();

            _table.Execute(TableOperation.InsertOrReplace(_entry));
        }
    }
}