using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TrueNorth.Azure.Extensions.CloudTableEx
{
    public static class CloudTableExtensions
    {
        public static async Task<IList<T>> ExecuteQueryAsync<T>(this TableClient tableClient, string tableName, Expression<Func<T, bool>> filter, List<string> columns, CancellationToken ct = default(CancellationToken), Action<IList<T>> onProgress = null) where T : class, ITableEntity, new()        {
            
            var items = new List<T>();

            string continuationToken = null;

            var pageable = tableClient.QueryAsync<T>(filter: filter, maxPerPage: 1000, select: columns, cancellationToken: ct);

            await using (IAsyncEnumerator<Page<T>> enumerator = pageable.AsPages(continuationToken).GetAsyncEnumerator())
            {
                await enumerator.MoveNextAsync();
                foreach (var item in enumerator.Current.Values)
                {
                    items.Add(item);
                }
            }

            return items;
        }
    }
}
