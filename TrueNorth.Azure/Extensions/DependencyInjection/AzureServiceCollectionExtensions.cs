using System;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TrueNorth.Azure.DocumentDb;

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
                            MaxRetryAttemptsOnThrottledRequests = 10,
                            MaxRetryWaitTimeInSeconds = 60
                        }
                    };
                    return new DocumentClient(new Uri(cosmosDbOptions.EndpointUri), cosmosDbOptions.PrimaryKey,
                        connectionPolicy);
                }
            );
        }

        public static void AddTableStorage(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<CloudTable>(s =>
            {
                var tableStorageOptions = s.GetService<IOptions<TableStorageOptions>>().Value;

                var azureLogConnection = tableStorageOptions.AzureTableStorageConnection;
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(azureLogConnection);
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable cloudtable = tableClient.GetTableReference("FastReserveLogs");

                var task = Task.Run(async () => await cloudtable.CreateIfNotExistsAsync());
                task.Wait();

                return cloudtable;

            });

            //serviceCollection.AddSingleton<ILoggerFactory>(s =>
            //{
            //    var tableStorageOptions = s.GetService<IOptions<TableStorageOptions>>().Value;
            //    var logWriter = s.GetService<BufferedLogWriter<LogEntryForWeb>>();
            //    var loggerFactory = new LoggerFactory();

            //    var minLogLevel = TrueNorth.Azure.LoggerExtensions.ParseLogLevel(tableStorageOptions.MinLevel);

            //    loggerFactory.AddProvider(new AzureTableStorageLoggerProvider(logWriter, minLogLevel));
            //    return loggerFactory;
            //});
        }

    }
}
