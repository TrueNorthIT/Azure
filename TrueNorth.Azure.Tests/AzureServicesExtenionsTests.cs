using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using TrueNorth.Azure.Extensions.Configuration;
using Xunit;

namespace TrueNorth.Azure.Tests
{
    public class AzureServicesExtenionsTests
    {
        [Fact]
        public void AzureConfigurationExtensions()
        {
            var expected = "endpoint";
            var options =
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("EndPoints:CosmosDB", "endpoint")
                };


            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(options);

            var configuration = builder.Build();

            var endpoint = configuration.GetEndPoint("CosmosDB");

            Assert.Equal(expected,endpoint);

        }
    }
}
