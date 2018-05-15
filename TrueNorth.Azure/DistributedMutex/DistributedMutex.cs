using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TrueNorth.Azure
{
    public class DistributedMutex
    {
        private DistributedMutexOptions Options { get; set; }
        private CloudBlobClient blobClient;

        public DistributedMutex(IOptions<DistributedMutexOptions> options)
        {
            this.Options = options.Value;

            var storageAccount = CloudStorageAccount.Parse(Options.StorageConnectionString);
            blobClient = storageAccount.CreateCloudBlobClient();

            var containerReference = blobClient.GetContainerReference(Options.ContainerName);
            containerReference.CreateIfNotExistsAsync().Wait();
            var blobReference = containerReference.GetBlockBlobReference(Options.Key);
            var task = blobReference.ExistsAsync();
            task.Wait();
            if (!task.Result)
            {
                blobReference.UploadTextAsync(string.Empty).Wait();
            }

        }

        /// <summary>
        ///     Acquires a lease blob.
        /// </summary>
        public async Task<string> AcquireAsync()
        {
            var containerReference = blobClient.GetContainerReference(Options.ContainerName);
            var blobReference = containerReference.GetBlockBlobReference(Options.Key);

            int tryCount = 0;

            string leaseId = null;
            while (tryCount < Options.Retries)
            {
                try
                {
                    tryCount++;
                    leaseId = await blobReference.AcquireLeaseAsync(TimeSpan.FromSeconds(Options.LeaseTimeSeconds));
                    if (!String.IsNullOrEmpty(leaseId))
                    {
                        return leaseId;
                    }
                }
                catch (StorageException ex)
                {
                    if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
                    {
                        await Task.Delay(Options.RetryWaitMs);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            throw new InvalidOperationException($"Could not obtain lease for {Options.Key}.");
        }


        /// <summary>
        ///     Releases a lease blob.
        /// </summary>  	
        public async Task ReleaseLockAsync(string leaseId)
        {
            var containerReference = blobClient.GetContainerReference(Options.ContainerName);
            var blobReference = containerReference.GetBlockBlobReference(Options.Key);

            await blobReference.ReleaseLeaseAsync(new AccessCondition
            {
                LeaseId = leaseId
            });
        }

        /// <summary>
        ///     Renews the lease.
        /// </summary>  	
        public async Task RenewAsync(string leaseId)
        {
            var containerClientReference = blobClient.GetContainerReference(Options.ContainerName);
            var blobReference = containerClientReference.GetBlockBlobReference(Options.Key);

            await blobReference.RenewLeaseAsync(new AccessCondition
            {
                LeaseId = leaseId
            });
        }
    }
}
