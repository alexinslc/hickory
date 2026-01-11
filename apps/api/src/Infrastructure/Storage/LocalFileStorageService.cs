using Microsoft.Extensions.Options;

namespace Hickory.Api.Infrastructure.Storage;

public class LocalFileStorageOptions
{
    public string StorageRootPath { get; set; } = "storage/attachments";
}

/// <summary>
/// Local file system implementation of file storage
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly LocalFileStorageOptions _options;
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _storageRoot;

    public LocalFileStorageService(
        IOptions<LocalFileStorageOptions> options,
        ILogger<LocalFileStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _storageRoot = Path.GetFullPath(_options.StorageRootPath);

        // Ensure storage directory exists
        if (!Directory.Exists(_storageRoot))
        {
            Directory.CreateDirectory(_storageRoot);
            _logger.LogInformation("Created storage directory at {StorageRoot}", _storageRoot);
        }
    }

    public async Task<string> UploadFileAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        // Generate unique file path to prevent collisions and enumeration attacks
        var fileId = Guid.NewGuid().ToString();
        var extension = Path.GetExtension(fileName);
        var sanitizedExtension = SanitizeExtension(extension);
        
        // Organize by date for better file system performance
        var datePath = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var directoryPath = Path.Combine(_storageRoot, datePath);
        
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var storagePath = Path.Combine(datePath, $"{fileId}{sanitizedExtension}");
        var fullPath = Path.Combine(_storageRoot, storagePath);

        try
        {
            using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            await stream.CopyToAsync(fileStream, cancellationToken);
            
            _logger.LogInformation("Uploaded file to {StoragePath}", storagePath);
            return storagePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file to {StoragePath}", storagePath);
            
            // Clean up partial file if it exists
            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogWarning(cleanupEx, "Failed to delete partial file {StoragePath}", storagePath);
                }
            }
            
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(storagePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {storagePath}");
        }

        try
        {
            // Return a FileStream that will be disposed by the caller
            var fileStream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81920,
                useAsync: true);

            return fileStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file from {StoragePath}", storagePath);
            throw;
        }
    }

    public Task DeleteFileAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(storagePath);

        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("Attempted to delete non-existent file: {StoragePath}", storagePath);
            return Task.CompletedTask;
        }

        try
        {
            File.Delete(fullPath);
            _logger.LogInformation("Deleted file {StoragePath}", storagePath);
            
            // Clean up empty directories
            CleanupEmptyDirectories(Path.GetDirectoryName(fullPath)!);
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file {StoragePath}", storagePath);
            throw;
        }
    }

    public Task<bool> FileExistsAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(storagePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    public Task<long> GetFileSizeAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(storagePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {storagePath}");
        }

        var fileInfo = new FileInfo(fullPath);
        return Task.FromResult(fileInfo.Length);
    }

    private string GetFullPath(string storagePath)
    {
        // Prevent directory traversal attacks
        var fullPath = Path.GetFullPath(Path.Combine(_storageRoot, storagePath));
        
        if (!fullPath.StartsWith(_storageRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid storage path");
        }

        return fullPath;
    }

    private static string SanitizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return string.Empty;
        }

        // Remove any path characters and limit length
        extension = extension.Replace("..", "").Replace("/", "").Replace("\\", "");
        return extension.Length > 10 ? extension.Substring(0, 10) : extension;
    }

    private void CleanupEmptyDirectories(string directory)
    {
        try
        {
            // Only clean up within our storage root
            if (!directory.StartsWith(_storageRoot, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
            {
                Directory.Delete(directory);
                _logger.LogDebug("Deleted empty directory {Directory}", directory);
                
                // Recursively clean up parent if empty
                var parent = Directory.GetParent(directory)?.FullName;
                if (parent != null && parent != _storageRoot)
                {
                    CleanupEmptyDirectories(parent);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanup empty directory {Directory}", directory);
        }
    }
}
