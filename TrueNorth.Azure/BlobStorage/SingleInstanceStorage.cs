using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using TrueNorth.Azure.Common;

namespace TrueNorth.Azure.BlobStorage
{
    public class SingleInstanceStorage
    {
        private readonly BlobContainerClient _cloudBlobContainer;
        private readonly SHA256AddressProvider _sha256AddressProvider;
        private volatile bool created;
        private readonly ILogger _logger;

        public SingleInstanceStorage(BlobContainerClient cloudBlobContainer, ILoggerFactory loggerFactory, SHA256AddressProvider sha256AddressProvider)
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
                await _cloudBlobContainer.CreateIfNotExistsAsync(publicAccessType: PublicAccessType.None);
                created = true;
            }

            var blockBlob = _cloudBlobContainer.GetBlobClient(address);
            bool exists = await blockBlob.ExistsAsync();

            //no need to check anything, as the address is the MD5 hash of the value
            if (!exists)
            {
                _logger.LogTrace($"WriteDocument: Creating blob {address}");
                await blockBlob.UploadAsync(stream, new BlobHttpHeaders() { ContentType = mimeType });
                
            }
            else if (mimeType != (await blockBlob.GetPropertiesAsync()).Value.ContentType)
            {
                await blockBlob.SetHttpHeadersAsync(new BlobHttpHeaders() { ContentType = mimeType });
            }
            return address;
        }

        public async Task<Stream> DownloadDocument(string address)
        {
            var blockBlob = _cloudBlobContainer.GetBlobClient(address);

            Stream rval = new MemoryStream();
            await blockBlob.DownloadToAsync(rval);
            rval.Position = 0;
            return rval;
        }
    }
}
