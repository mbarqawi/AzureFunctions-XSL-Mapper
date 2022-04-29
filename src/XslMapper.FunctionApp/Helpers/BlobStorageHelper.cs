using System;
using System.Threading.Tasks;

using Aliencube.XslMapper.FunctionApp.Configurations;
using Aliencube.XslMapper.FunctionApp.Exceptions;
using Aliencube.XslMapper.FunctionApp.Extensions;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Aliencube.XslMapper.FunctionApp.Helpers
{
    /// <summary>
    /// This represents the helper entity for Blob Storage.
    /// </summary>
    public class BlobStorageHelper : IBlobStorageHelper
    {
        private readonly AppSettings _settings;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStorageHelper"/> class.
        /// </summary>
        /// <param name="settings"><see cref="AppSettings"/> instance.</param>
        public BlobStorageHelper(AppSettings settings)
        {
            this._settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <inheritdoc />
        public async Task<CloudBlockBlob> LoadBlobAsync(string container, string directory, string filename)
        {
            if (!CloudStorageAccount.TryParse(this._settings.StorageConnectionString, out CloudStorageAccount account))
            {
                throw new CloudStorageNotFoundException();
            }

            var blob = await account.CreateBlobClient()
                                    .GetBlobContainerAsync(container)
                                    .GetBlockBlobAsync(filename, directory)
                                    .ConfigureAwait(false);

            return blob;
        }
        public async Task<string> UploadBlobAsync(string container, string directory, string fileName, byte[] fileBytes)
        {
            if (!CloudStorageAccount.TryParse(this._settings.StorageConnectionString, out CloudStorageAccount account))
            {
                throw new CloudStorageNotFoundException();
            }
            var cloudblobClient = account.CreateCloudBlobClient();
            var containerObject = cloudblobClient.GetContainerReference(container);
            var fileobject = containerObject.GetBlockBlobReference($"{directory}/{fileName}");
            //check the file type
            string file_type;
          
                file_type = "application/octet-stream";
           

            fileobject.Properties.ContentType = file_type;
            await fileobject.UploadFromByteArrayAsync(fileBytes, 0, fileBytes.Length);
            string fileuploadURI = fileobject.Uri.AbsoluteUri;
                        return fileuploadURI;
        }
        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Value indicating whther to be disposing resources or not.</param>
        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (this._disposed)
            {
                return;
            }

            this._disposed = true;
        }
    }
}