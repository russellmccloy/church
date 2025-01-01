namespace church_api.Services.Abstractions
{
    public interface IFileUploader
    {
        Task UploadFileAsync(string filePath, string blobName);
    }
}
