using System;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TrueNorth.Azure.BlobStorage;
using TrueNorth.Azure.Common;
using TrueNorth.Azure.DocumentDb;
using TrueNorth.Azure.Search;
using TrueNorth.Azure.TableStorage;

// ReSharper disable once CheckNamespace (following Microsofts style)
namespace TrueNorth.Extensions.DependencyInjection
{
    public static class AzureServiceCollectionExtensions
    {
        public static void AddDocumentDb(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton((s) =>
                {
                    var cosmosDbOptions = s.GetService<IOptions<DocumentDbOptions>>().Value;

                    var cosmosClientBuilder = new CosmosClientBuilder(cosmosDbOptions.EndpointUri, cosmosDbOptions.PrimaryKey)
                        .WithConnectionModeGateway(100)
                        .WithThrottlingRetryOptions(new TimeSpan(0, 0, cosmosDbOptions.MaxRetryWaitTimeInSeconds), cosmosDbOptions.MaxRetryAttemptsOnThrottledRequests);

                    return cosmosClientBuilder.Build();
                }
            );
        }

        public static void AddBlobStorage(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton((s) =>
                {
                    var options = s.GetService<IOptions<BlobStorageOptions>>().Value;
                    return new BlobContainerClient(options.ConnectionString, options.DefaultContainer);
                }
            );
        }

        public static void AddSingleInstanceStorage(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddBlobStorage();
            serviceCollection.AddSingleton<SHA256AddressProvider>();
            serviceCollection.AddSingleton<SingleInstanceStorage>();
        }        

        public static void AddTableStorage(this IServiceCollection serviceCollection, string tableName)
        {
            serviceCollection.AddSingleton <TableClient>(s =>
            {
                var tableStorageOptions = s.GetService<IOptions<TableStorageOptions>>().Value;

                var serviceClient = new TableServiceClient(new Uri(tableStorageOptions.AzureTableStorageConnection));
                var tableClient = serviceClient.GetTableClient(tableName);

                var task = Task.Run(async () => await tableClient.CreateIfNotExistsAsync());
                task.Wait();

                return tableClient;
            });
        }

        public static void AddSearchIndexClientService(this IServiceCollection serviceCollection, string index)
        {
            serviceCollection.AddSingleton((s) =>
            {
                var options = s.GetService<IOptions<SearchServiceOptions>>().Value;
                return new SearchIndexClient(
                    options.ServiceName, index, new SearchCredentials(options.ApiKey)
                    );

            }
          );
        }

        public static void AddSearchAdminService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton((s) =>
            {
                var options = s.GetService<IOptions<SearchServiceOptions>>().Value;
                return new SearchServiceClient(
                    options.ServiceName, new SearchCredentials(options.AdminKey)
                );
            }
            );
        }

        public static void AddTableStorage<T>(this IServiceCollection serviceCollection) where T : class, ICloudTableWrapper, new()
        {
            serviceCollection.AddSingleton<T>(s =>
            {
                T cloudTableWrapper = new T();

                var tableStorageOptions = s.GetService<IOptions<TableStorageOptions>>().Value;

                var serviceClient = new TableServiceClient(new Uri(tableStorageOptions.AzureTableStorageConnection));
                var tableClient = serviceClient.GetTableClient(default(T).TableName);

                var task = Task.Run(async () => await tableClient.CreateIfNotExistsAsync());
                task.Wait();

                cloudTableWrapper.CloudTableClient = tableClient;

                return cloudTableWrapper;

            });
        }
    }
}
