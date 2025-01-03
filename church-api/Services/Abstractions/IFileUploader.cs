using Azure.Storage.Blobs.Models;

namespace church_api.Services.Abstractions
{
    public interface IFileUploader
    {
        Task<Azure.Response<BlobContentInfo>> UploadFileAsync(string filePath, string blobName);
    }
}
