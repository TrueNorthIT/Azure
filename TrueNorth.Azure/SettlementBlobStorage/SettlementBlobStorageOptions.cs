using System;
using System.Collections.Generic;
using System.Text;

namespace TrueNorth.Azure
{
    public class SettlementBlobStorageOptions
    {
        public string StorageConnectionString { get; set; }
        public string ContainerName { get; set; }

    }
}
