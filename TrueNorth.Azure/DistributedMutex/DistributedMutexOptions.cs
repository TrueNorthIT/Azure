using System;
using System.Collections.Generic;
using System.Text;

namespace TrueNorth.Azure
{
    public class DistributedMutexOptions
    {
        public string StorageConnectionString { get; set; }
        public string ContainerName { get; set; }
        public string Key { get; set; }

        public int LeaseTimeSeconds { get; set; }
    }
}
