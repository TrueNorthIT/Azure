using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrueNorth.Azure.TableStorage
{
    public interface ICloudTableWrapper
    {
        CloudTable CloudTable { get; set; }

        string TableName { get; }
    }
}
