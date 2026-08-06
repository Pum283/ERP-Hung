namespace Erp.Application.DTOs.Prt;

public sealed record PrtPortalPackageDto(
    Guid Id, string PlanCode, string Name, string FeaturesJson,
    IReadOnlyDictionary<string, bool> Features, bool IsActive, string? Note);

public sealed record PrtPortalPackageUpsertRequest(
    Guid? Id, string PlanCode, string Name, string? FeaturesJson,
    IReadOnlyDictionary<string, bool>? Features, bool? IsActive, string? Note);

public sealed record PrtEnabledFeaturesDto(string PlanCode, IReadOnlyList<string> EnabledFeatures);
