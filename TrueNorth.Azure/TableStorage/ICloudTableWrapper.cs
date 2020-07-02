
using Microsoft.Azure.Cosmos.Table;

namespace TrueNorth.Azure.TableStorage
{
    public interface ICloudTableWrapper
    {
        CloudTable CloudTable { get; set; }

        string TableName { get; }
    }
}
