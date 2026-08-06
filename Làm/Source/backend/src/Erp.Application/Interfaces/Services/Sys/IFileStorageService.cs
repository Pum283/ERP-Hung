namespace Erp.Application.Interfaces.Services.Sys;

public sealed record StoredFileResult(
    string StorageKey,
    string FileName,
    long SizeBytes,
    string? PublicUrl);

public interface IFileStorageService
{
    /// <summary>Upload file. Folder gợi ý: brand | msg | docs.</summary>
    Task<StoredFileResult> SaveAsync(
        Stream content,
        string fileName,
        string? contentType,
        Guid tenantId,
        string? folder = null,
        CancellationToken ct = default);

    Task<(Stream Content, string FileName, string? ContentType)?> OpenReadAsync(
        string storageKey, Guid tenantId, CancellationToken ct = default);
}
