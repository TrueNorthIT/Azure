
using Azure.Data.Tables;

namespace TrueNorth.Azure.TableStorage
{
    public interface ICloudTableWrapper
    {
        TableClient CloudTableClient { get; set; }

        string TableName { get; }
    }
}
