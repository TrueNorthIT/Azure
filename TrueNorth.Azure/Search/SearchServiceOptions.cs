using System;
using System.Collections.Generic;
using System.Text;

namespace TrueNorth.Azure.Search
{
    public class SearchServiceOptions
    {
        public string ServiceName { get; set; }
        public string ApiKey { get; set; }
        public string Index { get; set; }
        public string AdminKey { get; set; }
    }
}
