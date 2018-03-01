using System;
using System.Collections.Generic;
using System.Text;

namespace TrueNorth.Azure.BlobStorage
{
    public class BlobStorageOptions
    {
        public string ConnectionString { get; set; }
        public string DefaultContainer { get; set; }
    }
}
