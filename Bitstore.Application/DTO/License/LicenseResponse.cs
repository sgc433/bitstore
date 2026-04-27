using Bitstore.Core.Enums;

namespace Bitstore.Application.DTO.License;

public record LicenseResponse(
    Guid Id,
    LicenseType Type,
    string Name,
    decimal Price,
    Guid BeatId
);