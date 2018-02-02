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
    }
}
