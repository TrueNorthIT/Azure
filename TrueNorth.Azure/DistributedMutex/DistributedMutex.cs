using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace TrueNorth.Azure
{
    public class DistributedMutex
    {
        private DistributedMutexOptions Options { get; set; }
        private BlobContainerClient blobClient;

        public DistributedMutex(IOptions<DistributedMutexOptions> options)
        {
            this.Options = options.Value;

            blobClient = new BlobContainerClient(Options.StorageConnectionString, Options.ContainerName);
            blobClient.CreateIfNotExists();
            var blobReference = blobClient.GetBlobClient(Options.Key);
            if (!blobReference.Exists())
            {
                blobReference.Upload(new MemoryStream());
            }

        }

        /// <summary>
        ///     Acquires a lease blob.
        /// </summary>
        public async Task<string> AcquireAsync(bool mustAcquire)
        {
            var blobReference = blobClient.GetBlobClient(Options.Key);
            var leaseClient = blobReference.GetBlobLeaseClient();

            int tryCount = 0;

            string leaseId = null;
            while (true)
            {
                try
                {
                    tryCount++;
                    leaseId = (await leaseClient.AcquireAsync(TimeSpan.FromSeconds(Options.LeaseTimeSeconds))).Value.LeaseId;
                    if (!String.IsNullOrEmpty(leaseId))
                    {
                        return leaseId;
                    }
                }
                catch (RequestFailedException ex)
                {
                    if (ex.Status == (int)HttpStatusCode.Conflict)
                    {
                        if (mustAcquire)
                        {
                            await Task.Delay(Options.RetryWaitMs);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }            
        }


        /// <summary>
        ///     Releases a lease blob.
        /// </summary>  	
        public async Task ReleaseLockAsync(string leaseId)
        {
            var blobReference = blobClient.GetBlobClient(Options.Key);
            var leaseClient = blobReference.GetBlobLeaseClient(leaseId);

            await leaseClient.ReleaseAsync();
        }

        /// <summary>
        ///     Renews the lease.
        /// </summary>  	
        public async Task RenewAsync(string leaseId)
        {
            var blobReference = blobClient.GetBlobClient(Options.Key);
            var leaseClient = blobReference.GetBlobLeaseClient(leaseId);

            await leaseClient.RenewAsync();
        }
    }
}
