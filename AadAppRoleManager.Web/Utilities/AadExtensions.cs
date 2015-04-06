using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;

namespace AadAppRoleManager.Web.Utilities
{
    public static class AadExtensions
    {
        public static async Task<IEnumerable<IUser>> GetAllUsersInGroupsAsync(
            this IActiveDirectoryClient client,
            IEnumerable<string> groupObjectIds)
        {
            var groupMembers =
                (await (await groupObjectIds
                    .Select(id => client.Groups.GetByObjectId(id).Members.ExecuteAsync())
                    .WhenAll())
                    .Select(c => c.EnumerateAllAsync())
                    .WhenAll())
                    .SelectMany(m => m);

            return groupMembers.OfType<IUser>().ToList();
        }

        public static Task<IEnumerable<T>> EnumerateAllAsync<T>(
            this IPagedCollection<T> pagedCollection)
        {
            return EnumerateAllAsync(pagedCollection, Enumerable.Empty<T>());
        }

        private static async Task<IEnumerable<T>> EnumerateAllAsync<T>(
            this IPagedCollection<T> pagedCollection,
            IEnumerable<T> previousItems)
        {
            var newPreviousItems = previousItems.Concat(pagedCollection.CurrentPage);

            if (pagedCollection.MorePagesAvailable == false)
                return newPreviousItems;

            var newPagedCollection = await pagedCollection.GetNextPageAsync();
            return await EnumerateAllAsync(newPagedCollection, newPreviousItems);
        }
    }
}