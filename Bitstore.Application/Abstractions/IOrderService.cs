using Bitstore.Application.DTO.Order;
using Bitstore.Core.Enums;

namespace Bitstore.Application.Abstractions;

public interface IOrderService
{
    Task<OrderResponse> GetOrderById(Guid orderId);
    Task<List<OrderResponse>> GetOrdersByBuyer(Guid buyerId);
    Task<List<OrderResponse>> GetOrdersBySeller(Guid sellerId);
    Task<OrderResponse> CreateOrder(OrderRequest request);
    Task<OrderResponse> UpdateOrderStatus(Guid orderId, OrderStatus status);
    Task<bool> CancelOrder(Guid orderId);
}