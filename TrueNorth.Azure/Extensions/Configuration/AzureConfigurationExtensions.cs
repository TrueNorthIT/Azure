using Microsoft.Extensions.Configuration;

namespace TrueNorth.Azure.Extensions.Configuration
{
    public static class AzureConfigurationExtensions
    {
        public static string GetEndPoint(this IConfiguration configuration, string name)
        {
            return configuration.GetSection("EndPoints")[name];
        }
    }
}
