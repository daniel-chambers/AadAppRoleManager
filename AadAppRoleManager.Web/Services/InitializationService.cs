using Autofac;
using Microsoft.WindowsAzure.Storage;

namespace AadAppRoleManager.Web.Services
{
    public class InitializationService : IStartable
    {
        private readonly CloudStorageAccount _storageAccount;

        public InitializationService(CloudStorageAccount storageAccount)
        {
            _storageAccount = storageAccount;
        }

        public void Start()
        {
            var tableClient = _storageAccount.CreateCloudTableClient();
            tableClient.GetTableReference(TableNames.AadUserTokenCacheEntries).CreateIfNotExistsAsync().Wait();
        }
    }
}