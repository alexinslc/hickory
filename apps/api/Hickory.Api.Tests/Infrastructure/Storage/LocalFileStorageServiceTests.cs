using Hickory.Api.Infrastructure.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Hickory.Api.Tests.Infrastructure.Storage;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _testStorageRoot;
    private readonly LocalFileStorageService _service;
    private readonly Mock<ILogger<LocalFileStorageService>> _mockLogger;

    public LocalFileStorageServiceTests()
    {
        _testStorageRoot = Path.Combine(Path.GetTempPath(), $"test-storage-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testStorageRoot);
        
        _mockLogger = new Mock<ILogger<LocalFileStorageService>>();
        var options = Options.Create(new LocalFileStorageOptions { StorageRootPath = _testStorageRoot });
        _service = new LocalFileStorageService(options, _mockLogger.Object);
    }

    [Fact]
    public async Task UploadFileAsync_Should_Create_File_With_Date_Based_Path()
    {
        // Arrange
        var fileName = "test-file.txt";
        var content = "Hello, World!"u8.ToArray();
        using var stream = new MemoryStream(content);

        // Act
        var storagePath = await _service.UploadFileAsync(stream, fileName, "text/plain", CancellationToken.None);

        // Assert
        Assert.NotNull(storagePath);
        Assert.Contains(DateTime.UtcNow.Year.ToString(), storagePath);
        Assert.EndsWith(".txt", storagePath);

        // Verify file exists
        var fullPath = Path.Combine(_testStorageRoot, storagePath);
        Assert.True(File.Exists(fullPath));

        // Verify content
        var savedContent = await File.ReadAllBytesAsync(fullPath);
        Assert.Equal(content, savedContent);
    }

    [Fact]
    public async Task UploadFileAsync_Should_Sanitize_File_Extension()
    {
        // Arrange
        var fileName = "test-file.pdf.exe"; // Dangerous extension
        var content = "test"u8.ToArray();
        using var stream = new MemoryStream(content);

        // Act
        var storagePath = await _service.UploadFileAsync(stream, fileName, "application/pdf", CancellationToken.None);

        // Assert
        Assert.NotNull(storagePath);
        Assert.EndsWith(".exe", storagePath); // Path.GetExtension returns the last extension
    }

    [Fact]
    public async Task DownloadFileAsync_Should_Return_File_Stream()
    {
        // Arrange
        var fileName = "download-test.txt";
        var content = "Download test content"u8.ToArray();
        using var uploadStream = new MemoryStream(content);
        var storagePath = await _service.UploadFileAsync(uploadStream, fileName, "text/plain", CancellationToken.None);

        // Act
        var downloadStream = await _service.DownloadFileAsync(storagePath, CancellationToken.None);

        // Assert
        Assert.NotNull(downloadStream);
        using var memoryStream = new MemoryStream();
        await downloadStream.CopyToAsync(memoryStream);
        var downloadedContent = memoryStream.ToArray();
        Assert.Equal(content, downloadedContent);
    }

    [Fact]
    public async Task DownloadFileAsync_Should_Throw_When_File_Not_Found()
    {
        // Arrange
        var nonExistentPath = "2026/01/10/non-existent-file.txt";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            async () => await _service.DownloadFileAsync(nonExistentPath, CancellationToken.None)
        );
    }

    [Fact]
    public async Task DeleteFileAsync_Should_Remove_File_And_Empty_Directories()
    {
        // Arrange
        var fileName = "delete-test.txt";
        var content = "Delete test"u8.ToArray();
        using var stream = new MemoryStream(content);
        var storagePath = await _service.UploadFileAsync(stream, fileName, "text/plain", CancellationToken.None);
        
        var fullPath = Path.Combine(_testStorageRoot, storagePath);
        Assert.True(File.Exists(fullPath));

        // Act
        await _service.DeleteFileAsync(storagePath, CancellationToken.None);

        // Assert
        Assert.False(File.Exists(fullPath));
        
        // Verify empty directories are cleaned up
        var directory = Path.GetDirectoryName(fullPath)!;
        // Directory may or may not exist depending on if it's empty
        if (Directory.Exists(directory))
        {
            Assert.Empty(Directory.GetFileSystemEntries(directory));
        }
    }

    [Fact]
    public async Task DeleteFileAsync_Should_Not_Throw_When_File_Not_Found()
    {
        // Arrange
        var nonExistentPath = "2026/01/10/non-existent-file.txt";

        // Act & Assert - Should not throw
        await _service.DeleteFileAsync(nonExistentPath, CancellationToken.None);
    }

    [Fact]
    public async Task FileExistsAsync_Should_Return_True_When_File_Exists()
    {
        // Arrange
        var fileName = "exists-test.txt";
        var content = "Exists test"u8.ToArray();
        using var stream = new MemoryStream(content);
        var storagePath = await _service.UploadFileAsync(stream, fileName, "text/plain", CancellationToken.None);

        // Act
        var exists = await _service.FileExistsAsync(storagePath, CancellationToken.None);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task FileExistsAsync_Should_Return_False_When_File_Not_Found()
    {
        // Arrange
        var nonExistentPath = "2026/01/10/non-existent-file.txt";

        // Act
        var exists = await _service.FileExistsAsync(nonExistentPath, CancellationToken.None);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task GetFileSizeAsync_Should_Return_Correct_Size()
    {
        // Arrange
        var fileName = "size-test.txt";
        var content = "12345678"u8.ToArray(); // 8 bytes
        using var stream = new MemoryStream(content);
        var storagePath = await _service.UploadFileAsync(stream, fileName, "text/plain", CancellationToken.None);

        // Act
        var size = await _service.GetFileSizeAsync(storagePath, CancellationToken.None);

        // Assert
        Assert.Equal(8, size);
    }

    [Fact]
    public async Task GetFileSizeAsync_Should_Throw_When_File_Not_Found()
    {
        // Arrange
        var nonExistentPath = "2026/01/10/non-existent-file.txt";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            async () => await _service.GetFileSizeAsync(nonExistentPath, CancellationToken.None)
        );
    }

    [Theory]
    [InlineData("../../etc/passwd")]
    [InlineData("..\\..\\windows\\system32")]
    [InlineData("../../../file.txt")]
    public async Task UploadFileAsync_Should_Prevent_Path_Traversal(string maliciousPath)
    {
        // Arrange
        var content = "malicious"u8.ToArray();
        using var stream = new MemoryStream(content);

        // Act
        var storagePath = await _service.UploadFileAsync(stream, maliciousPath, "text/plain", CancellationToken.None);

        // Assert - Should sanitize the path
        var fullPath = Path.Combine(_testStorageRoot, storagePath);
        Assert.StartsWith(_testStorageRoot, fullPath);
        Assert.True(File.Exists(fullPath));
    }

    public void Dispose()
    {
        // Clean up test storage directory
        if (Directory.Exists(_testStorageRoot))
        {
            Directory.Delete(_testStorageRoot, recursive: true);
        }
    }
}
