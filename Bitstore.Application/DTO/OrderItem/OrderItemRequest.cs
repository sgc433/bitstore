namespace Bitstore.Application.DTO.OrderItem;

public record OrderItemRequest(
    Guid BeatId,
    Guid LicenseId
);