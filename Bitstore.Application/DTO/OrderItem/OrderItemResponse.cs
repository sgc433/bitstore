using Bitstore.Core.Enums;

namespace Bitstore.Application.DTO.OrderItem;

public record OrderItemResponse(
    Guid Id,
    decimal Price,
    Guid BeatId,
    string BeatTitle,
    Guid LicenseId,
    LicenseType LicenseType,
    Guid SellerId,
    string SellerName
);