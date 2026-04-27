using Bitstore.Application.DTO.OrderItem;
using Bitstore.Core.Models;

namespace Bitstore.Application.DTO.Order;

public record OrderRequest(
    List<OrderItemRequest>  Items
    );