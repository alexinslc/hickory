namespace Hickory.Api.Infrastructure.Storage;

/// <summary>
/// Abstraction for file storage operations
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to storage
    /// </summary>
    /// <param name="stream">File content stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="contentType">MIME type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Unique storage path for the file</returns>
    Task<string> UploadFileAsync(
        Stream stream, 
        string fileName, 
        string contentType, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file from storage
    /// </summary>
    /// <param name="storagePath">Storage path returned from upload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File stream</returns>
    Task<Stream> DownloadFileAsync(
        string storagePath, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    /// <param name="storagePath">Storage path to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteFileAsync(
        string storagePath, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists in storage
    /// </summary>
    /// <param name="storagePath">Storage path to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if file exists</returns>
    Task<bool> FileExistsAsync(
        string storagePath, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the size of a file in bytes
    /// </summary>
    /// <param name="storagePath">Storage path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File size in bytes</returns>
    Task<long> GetFileSizeAsync(
        string storagePath, 
        CancellationToken cancellationToken = default);
}
