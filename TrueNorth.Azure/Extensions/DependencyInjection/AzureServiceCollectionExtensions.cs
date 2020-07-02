using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos.Table;
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

                    var connectionPolicy = new ConnectionPolicy
                    {
                        MaxConnectionLimit = 100,
                        ConnectionMode = ConnectionMode.Gateway,
                        RetryOptions =
                        {
                            MaxRetryAttemptsOnThrottledRequests = cosmosDbOptions.MaxRetryAttemptsOnThrottledRequests,
                            MaxRetryWaitTimeInSeconds = cosmosDbOptions.MaxRetryWaitTimeInSeconds
                        }
                    };
                    return new DocumentClient(new Uri(cosmosDbOptions.EndpointUri), cosmosDbOptions.PrimaryKey,
                        connectionPolicy);
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

        

        public static void AddTableStorage(this IServiceCollection serviceCollection, string logTableName)
        {
            serviceCollection.AddSingleton<CloudTable>(s =>
            {
                var tableStorageOptions = s.GetService<IOptions<TableStorageOptions>>().Value;

                var azureLogConnection = tableStorageOptions.AzureTableStorageConnection;
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(azureLogConnection);
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable cloudtable = tableClient.GetTableReference(logTableName);

                var task = Task.Run(async () => await cloudtable.CreateIfNotExistsAsync());
                task.Wait();

                return cloudtable;

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

                var azureLogConnection = tableStorageOptions.AzureTableStorageConnection;
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(azureLogConnection);
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable cloudtable = tableClient.GetTableReference(cloudTableWrapper.TableName);

                var task = Task.Run(async () => await cloudtable.CreateIfNotExistsAsync());
                task.Wait();

                cloudTableWrapper.CloudTable = cloudtable;

                return cloudTableWrapper;

            });
        }
    }
}
