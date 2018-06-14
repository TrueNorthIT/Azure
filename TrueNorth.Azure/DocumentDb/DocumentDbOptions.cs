using System;
using System.Collections.Generic;
using System.Text;

namespace TrueNorth.Azure.DocumentDb
{
    public class DocumentDbOptions
    {
        public string EndpointUri { get; set; }
        public string PrimaryKey { get; set; }
        public int MaxRetryAttemptsOnThrottledRequests { get; set; } = 10;
        public int MaxRetryWaitTimeInSeconds { get; set; } = 60;
    }
}
