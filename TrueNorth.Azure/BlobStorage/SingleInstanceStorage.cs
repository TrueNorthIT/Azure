using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using TrueNorth.Azure.Common;

namespace TrueNorth.Azure.BlobStorage
{
    public class SingleInstanceStorage
    {
        private readonly CloudBlobContainer _cloudBlobContainer;
        private readonly SHA256AddressProvider _sha256AddressProvider;
        private volatile bool created;
        private readonly ILogger _logger;

        public SingleInstanceStorage(CloudBlobContainer cloudBlobContainer, ILoggerFactory loggerFactory, SHA256AddressProvider sha256AddressProvider)
        {
            _cloudBlobContainer = cloudBlobContainer;
            _sha256AddressProvider = sha256AddressProvider;
            _logger = loggerFactory.CreateLogger<SingleInstanceStorage>();
        }

        public async Task<string> WriteDocument(Stream stream, string extension)
        {
            var address = _sha256AddressProvider.GenerateHash(stream);
            var mimeType = MimeTypeMap.GetMimeType(extension);

            _logger.LogTrace($"WriteDocument(stream,{address})");
            
            if (!created)
            {
                // CreateIfNotExistsAsync is thread safe, so no need to lock.
                await _cloudBlobContainer.CreateIfNotExistsAsync();
                await _cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Off
                });
                created = true;
            }

            var blockBlob = _cloudBlobContainer.GetBlockBlobReference(address);
            bool exists = await blockBlob.ExistsAsync();

            //no need to check anything, as the address is the MD5 hash of the value
            if (!exists)
            {
                _logger.LogTrace($"WriteDocument: Creating blob {address}");
                await blockBlob.UploadFromStreamAsync(stream);
                
            }
            if (mimeType != blockBlob.Properties.ContentType || !exists)
            {
                blockBlob.Properties.ContentType = mimeType;
                await blockBlob.SetPropertiesAsync();
            }
            blockBlob.Properties.ContentType = mimeType;
            return address;
        }

        public async Task<Stream> DownloadDocument(string address)
        {
            var blockBlob = _cloudBlobContainer.GetBlockBlobReference(address);

            Stream rval = new MemoryStream();
            await blockBlob.DownloadToStreamAsync(rval);
            rval.Position = 0;
            return rval;
        }
    }
}
