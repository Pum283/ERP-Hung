using Erp.Application.Interfaces.Services.Sys;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Erp.Infrastructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _root;
    private readonly string? _publicBase;

    public LocalFileStorageService(IConfiguration config, IHostEnvironment env)
    {
        var configured = config["Storage:LocalRoot"];
        _root = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(env.ContentRootPath, "App_Data", "files")
            : configured;
        _publicBase = config["Storage:PublicBaseUrl"]?.Trim().TrimEnd('/');
        Directory.CreateDirectory(_root);
    }

    public async Task<StoredFileResult> SaveAsync(
        Stream content, string fileName, string? contentType, Guid tenantId,
        string? folder = null, CancellationToken ct = default)
    {
        var safeName = Path.GetFileName(fileName);
        var sub = string.IsNullOrWhiteSpace(folder) ? "files" : folder.Trim().Trim('/');
        var key = $"{tenantId:N}/{sub}/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}_{safeName}";
        var fullPath = Path.Combine(_root, key.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fs = File.Create(fullPath);
        await content.CopyToAsync(fs, ct);
        var size = fs.Length;
        var url = _publicBase is null ? null : $"{_publicBase}/{key}";
        return new StoredFileResult(key, safeName, size, url);
    }

    public Task<(Stream Content, string FileName, string? ContentType)?> OpenReadAsync(
        string storageKey, Guid tenantId, CancellationToken ct = default)
    {
        if (storageKey.StartsWith("cloudinary:", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<(Stream, string, string?)?>(null);
        if (storageKey.Contains("..", StringComparison.Ordinal) ||
            !storageKey.StartsWith($"{tenantId:N}/", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<(Stream, string, string?)?>(null);

        var fullPath = Path.Combine(_root, storageKey.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
            return Task.FromResult<(Stream, string, string?)?>(null);

        Stream stream = File.OpenRead(fullPath);
        var name = Path.GetFileName(fullPath);
        return Task.FromResult<(Stream, string, string?)?>((stream, name, null));
    }
}
