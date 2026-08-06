using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Erp.Application.Interfaces.Services.Sys;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Erp.Infrastructure.Storage;

/// <summary>Lưu file lên Cloudinary (signed upload). StorageKey = cloudinary:{public_id}</summary>
public sealed class CloudinaryFileStorageService : IFileStorageService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<CloudinaryFileStorageService> _log;
    private readonly string _cloud;
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly string _folderRoot;

    public CloudinaryFileStorageService(
        IHttpClientFactory httpFactory,
        IConfiguration config,
        ILogger<CloudinaryFileStorageService> log)
    {
        _httpFactory = httpFactory;
        _log = log;
        _cloud = config["CLOUDINARY_CLOUD_NAME"]?.Trim()
                 ?? throw new InvalidOperationException("Thiếu CLOUDINARY_CLOUD_NAME");
        _apiKey = config["CLOUDINARY_API_KEY"]?.Trim()
                  ?? throw new InvalidOperationException("Thiếu CLOUDINARY_API_KEY");
        _apiSecret = config["CLOUDINARY_API_SECRET"]?.Trim()
                     ?? throw new InvalidOperationException("Thiếu CLOUDINARY_API_SECRET");
        _folderRoot = (config["CLOUDINARY_FOLDER"] ?? "pums-erp").Trim().Trim('/');
    }

    public async Task<StoredFileResult> SaveAsync(
        Stream content, string fileName, string? contentType, Guid tenantId,
        string? folder = null, CancellationToken ct = default)
    {
        var safeName = Path.GetFileName(fileName);
        var sub = string.IsNullOrWhiteSpace(folder) ? "files" : folder.Trim().Trim('/');
        var folderPath = $"{_folderRoot}/{tenantId:N}/{sub}";
        var leafId = $"{Guid.NewGuid():N}_{Path.GetFileNameWithoutExtension(safeName)}";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        // Signed params (alphabetical): folder, public_id, timestamp
        var toSign = $"folder={folderPath}&public_id={leafId}&timestamp={timestamp}{_apiSecret}";
        var signature = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(toSign))).ToLowerInvariant();

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(_apiKey), "api_key");
        form.Add(new StringContent(timestamp), "timestamp");
        form.Add(new StringContent(signature), "signature");
        form.Add(new StringContent(folderPath), "folder");
        form.Add(new StringContent(leafId), "public_id");

        // Buffer stream — Cloudinary needs known length for StreamContent
        await using var ms = new MemoryStream();
        await content.CopyToAsync(ms, ct);
        var size = ms.Length;
        ms.Position = 0;
        var fileContent = new StreamContent(ms);
        if (!string.IsNullOrWhiteSpace(contentType))
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        form.Add(fileContent, "file", safeName);

        var client = _httpFactory.CreateClient("cloudinary");
        var url = $"https://api.cloudinary.com/v1_1/{_cloud}/auto/upload";
        using var resp = await client.PostAsync(url, form, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            _log.LogError("Cloudinary upload failed {Status}: {Body}", (int)resp.StatusCode, body);
            throw new InvalidOperationException("Upload Cloudinary thất bại.");
        }

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        var returnedId = root.GetProperty("public_id").GetString() ?? $"{folderPath}/{leafId}";
        string? secureUrl = null;
        if (root.TryGetProperty("secure_url", out var su)) secureUrl = su.GetString();
        if (secureUrl is null && root.TryGetProperty("url", out var u)) secureUrl = u.GetString();

        return new StoredFileResult($"cloudinary:{returnedId}", safeName, size, secureUrl);
    }

    public async Task<(Stream Content, string FileName, string? ContentType)?> OpenReadAsync(
        string storageKey, Guid tenantId, CancellationToken ct = default)
    {
        if (!storageKey.StartsWith("cloudinary:", StringComparison.OrdinalIgnoreCase))
            return null;
        var publicId = storageKey["cloudinary:".Length..];
        if (!publicId.Contains(tenantId.ToString("N"), StringComparison.OrdinalIgnoreCase))
            return null;

        var delivery = $"https://res.cloudinary.com/{_cloud}/image/upload/{publicId}";
        var client = _httpFactory.CreateClient("cloudinary");
        var resp = await client.GetAsync(delivery, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!resp.IsSuccessStatusCode) return null;
        var stream = await resp.Content.ReadAsStreamAsync(ct);
        var name = Path.GetFileName(publicId);
        var ctype = resp.Content.Headers.ContentType?.MediaType;
        return (stream, name, ctype);
    }
}
