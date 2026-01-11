using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Hickory.Api.IntegrationTests.TestFixtures;
using Hickory.Api.Infrastructure.Auth;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Hickory.Api.IntegrationTests.Features.Attachments;

public class AttachmentsControllerTests : IClassFixture<ApiWebApplicationFactory>, IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private ApplicationDbContext _dbContext = null!;
    private string _authToken = string.Empty;
    private Guid _userId;
    private Guid _ticketId;

    public AttachmentsControllerTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // Create test user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "attachment-test@example.com",
            FirstName = "Attachment",
            LastName = "Tester",
            Role = UserRole.EndUser,
            PasswordHash = passwordHasher.HashPassword("Password123!"),
            CreatedAt = DateTime.UtcNow
        };
        _userId = user.Id;

        // Create test ticket
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            TicketNumber = $"HSD-{DateTime.UtcNow:yyyyMMdd}-9999",
            Title = "Test Ticket for Attachments",
            Description = "Testing file attachments",
            Status = TicketStatus.Open,
            Priority = TicketPriority.Medium,
            SubmitterId = user.Id,
            CreatedAt = DateTime.UtcNow
        };
        _ticketId = ticket.Id;

        _dbContext.Users.Add(user);
        _dbContext.Tickets.Add(ticket);
        await _dbContext.SaveChangesAsync();

        // Login to get auth token
        var loginRequest = new
        {
            email = user.Email,
            password = "Password123!"
        };

        using var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadAsStringAsync();
        var loginData = JsonSerializer.Deserialize<JsonElement>(loginResult);
        _authToken = loginData.GetProperty("accessToken").GetString()!;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
    }

    [Fact]
    public async Task UploadAttachment_Should_Return_201_With_Valid_File()
    {
        // Arrange
        var fileContent = "This is a test file content"u8.ToArray();
        var fileName = "test-document.txt";
        
        using var content = new MultipartFormDataContent();
        using var fileStream = new MemoryStream(fileContent);
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(streamContent, "file", fileName);

        // Act
        var response = await _client.PostAsync($"/api/attachments/tickets/{_ticketId}", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync();
        var attachment = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.True(attachment.TryGetProperty("id", out var id));
        Assert.NotEqual(Guid.Empty.ToString(), id.GetString());
        Assert.Equal(fileName, attachment.GetProperty("fileName").GetString());
        Assert.Equal("text/plain", attachment.GetProperty("contentType").GetString());
        Assert.Equal(fileContent.Length, attachment.GetProperty("fileSizeBytes").GetInt64());
        Assert.Equal(_userId.ToString(), attachment.GetProperty("uploadedById").GetString());
    }

    [Fact]
    public async Task UploadAttachment_Should_Return_400_With_No_File()
    {
        // Arrange
        using var content = new MultipartFormDataContent();

        // Act
        var response = await _client.PostAsync($"/api/attachments/tickets/{_ticketId}", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadAttachment_Should_Return_400_With_File_Too_Large()
    {
        // Arrange
        var largeFile = new byte[11 * 1024 * 1024]; // 11 MB
        var fileName = "large-file.pdf";
        
        using var content = new MultipartFormDataContent();
        using var fileStream = new MemoryStream(largeFile);
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(streamContent, "file", fileName);

        // Act
        var response = await _client.PostAsync($"/api/attachments/tickets/{_ticketId}", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadAttachment_Should_Return_400_With_Invalid_File_Type()
    {
        // Arrange
        var fileContent = "malicious executable"u8.ToArray();
        var fileName = "malware.exe";
        
        using var content = new MultipartFormDataContent();
        using var fileStream = new MemoryStream(fileContent);
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-msdownload");
        content.Add(streamContent, "file", fileName);

        // Act
        var response = await _client.PostAsync($"/api/attachments/tickets/{_ticketId}", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DownloadAttachment_Should_Return_File()
    {
        // Arrange - Upload a file first
        var fileContent = "Download test content"u8.ToArray();
        var fileName = "download-test.txt";
        
        using var uploadContent = new MultipartFormDataContent();
        using var fileStream = new MemoryStream(fileContent);
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        uploadContent.Add(streamContent, "file", fileName);

        var uploadResponse = await _client.PostAsync($"/api/attachments/tickets/{_ticketId}", uploadContent);
        uploadResponse.EnsureSuccessStatusCode();

        var uploadResponseBody = await uploadResponse.Content.ReadAsStringAsync();
        var attachment = JsonSerializer.Deserialize<JsonElement>(uploadResponseBody);
        var attachmentId = attachment.GetProperty("id").GetString();

        // Act - Download the file
        var downloadResponse = await _client.GetAsync($"/api/attachments/{attachmentId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, downloadResponse.StatusCode);
        Assert.Equal("text/plain", downloadResponse.Content.Headers.ContentType?.MediaType);

        var downloadedContent = await downloadResponse.Content.ReadAsByteArrayAsync();
        Assert.Equal(fileContent, downloadedContent);
    }

    [Fact]
    public async Task DownloadAttachment_Should_Return_404_When_Not_Found()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/attachments/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAttachment_Should_Return_204()
    {
        // Arrange - Upload a file first
        var fileContent = "Delete test content"u8.ToArray();
        var fileName = "delete-test.txt";
        
        using var uploadContent = new MultipartFormDataContent();
        using var fileStream = new MemoryStream(fileContent);
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        uploadContent.Add(streamContent, "file", fileName);

        var uploadResponse = await _client.PostAsync($"/api/attachments/tickets/{_ticketId}", uploadContent);
        uploadResponse.EnsureSuccessStatusCode();

        var uploadResponseBody = await uploadResponse.Content.ReadAsStringAsync();
        var attachment = JsonSerializer.Deserialize<JsonElement>(uploadResponseBody);
        var attachmentId = attachment.GetProperty("id").GetString();

        // Act - Delete the attachment
        var deleteResponse = await _client.DeleteAsync($"/api/attachments/{attachmentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify attachment no longer exists
        var getResponse = await _client.GetAsync($"/api/attachments/{attachmentId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteAttachment_Should_Return_404_When_Not_Found()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/attachments/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Upload_And_Download_Multiple_Attachments()
    {
        // Arrange & Act - Upload multiple files
        var files = new[]
        {
            ("file1.txt", "First file content"u8.ToArray(), "text/plain"),
            ("file2.pdf", "PDF content"u8.ToArray(), "application/pdf"),
            ("file3.png", "PNG content"u8.ToArray(), "image/png")
        };

        var attachmentIds = new List<string>();

        foreach (var (name, content, contentType) in files)
        {
            using var uploadContent = new MultipartFormDataContent();
            using var fileStream = new MemoryStream(content);
            using var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            uploadContent.Add(streamContent, "file", name);

            var uploadResponse = await _client.PostAsync($"/api/attachments/tickets/{_ticketId}", uploadContent);
            uploadResponse.EnsureSuccessStatusCode();

            var responseBody = await uploadResponse.Content.ReadAsStringAsync();
            var attachment = JsonSerializer.Deserialize<JsonElement>(responseBody);
            attachmentIds.Add(attachment.GetProperty("id").GetString()!);
        }

        // Assert - Verify all files can be downloaded
        Assert.Equal(3, attachmentIds.Count);

        for (int i = 0; i < files.Length; i++)
        {
            var downloadResponse = await _client.GetAsync($"/api/attachments/{attachmentIds[i]}");
            Assert.Equal(HttpStatusCode.OK, downloadResponse.StatusCode);

            var downloadedContent = await downloadResponse.Content.ReadAsByteArrayAsync();
            Assert.Equal(files[i].Item2, downloadedContent);
        }
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }
}
