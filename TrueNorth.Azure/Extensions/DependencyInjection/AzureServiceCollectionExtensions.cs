using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using TrueNorth.Azure.BlobStorage;
using TrueNorth.Azure.Common;
using TrueNorth.Azure.DocumentDb;
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

                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        options.ConnectionString
                    );

                    var containerName = options.DefaultContainer;
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    return blobClient.GetContainerReference(containerName);
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
