using Bitstore.Application.DTO.OrderItem;
using Bitstore.Core.Enums;

namespace Bitstore.Application.DTO.Order;

public record OrderResponse(
    Guid Id,
    string OrderNumber,
    Guid BuyerId,
    decimal TotalAmount,
    OrderStatus Status,
    DateTime CreatedAt,
    List<OrderItemResponse> Items
);