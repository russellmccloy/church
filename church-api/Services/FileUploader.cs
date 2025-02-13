﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using church_api.Services.Abstractions;

namespace church_api.Services
{
    public class FileUploader : IFileUploader
    {
        private readonly BlobServiceClient _blobServiceClient;

        public FileUploader(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<Azure.Response<BlobContentInfo>> UploadFileAsync(string filePath, string blobName)
        {
            string containerName = "images";

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            await blobContainerClient.CreateIfNotExistsAsync();

            var blobClient = blobContainerClient.GetBlobClient(blobName);

            var uploaded = await blobClient.UploadAsync(filePath, overwrite: true);

            return uploaded;
        }
    }
}
