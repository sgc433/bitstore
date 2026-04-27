using Bitstore.Core.Enums;

namespace Bitstore.Application.DTO.License;

public record LicenseRequest(
    LicenseType Type,
    string Name,
    decimal Price
);