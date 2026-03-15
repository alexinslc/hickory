namespace Hickory.Api.Infrastructure.Middleware;

/// <summary>
/// Middleware that disables response compression for responses below a minimum size threshold.
/// Responses smaller than the configured minimum (default 1 KB) have their compression headers
/// stripped so that the client receives the original uncompressed body. This avoids unnecessary
/// CPU overhead when the size savings would be negligible.
/// Place this middleware immediately after UseResponseCompression in the pipeline.
/// </summary>
public class MinimumSizeCompressionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly long _minimumBodySizeBytes;

    /// <summary>
    /// Initializes a new instance of MinimumSizeCompressionMiddleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="minimumBodySizeBytes">
    /// Minimum response body size in bytes required for compression to remain active.
    /// Responses smaller than this value will have compression headers removed.
    /// Defaults to 1024 (1 KB).
    /// </param>
    public MinimumSizeCompressionMiddleware(RequestDelegate next, long minimumBodySizeBytes = 1024)
    {
        _next = next;
        _minimumBodySizeBytes = minimumBodySizeBytes;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        await _next(context);

        buffer.Seek(0, SeekOrigin.Begin);

        if (buffer.Length < _minimumBodySizeBytes)
        {
            // Strip compression headers for small responses
            context.Response.Headers.Remove("Content-Encoding");
            context.Response.Headers.ContentLength = buffer.Length;
        }

        context.Response.Body = originalBody;
        await buffer.CopyToAsync(originalBody);
    }
}
