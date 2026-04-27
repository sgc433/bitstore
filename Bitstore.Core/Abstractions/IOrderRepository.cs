using Bitstore.Core.Models;

namespace Bitstore.Core.Abstractions;

public interface IOrderRepository
{
    Task<List<Order>> GetAllAsync();
    Task<Order> GetByIdAsync(Guid orderId);
    Task<Order> GetByIdWithDetailsAsync(Guid orderId);
    Task<List<Order>> GetByBuyerIdAsync(Guid buyerId);
    Task<List<Order>> GetBySellerIdAsync(Guid sellerId);
    Task CreateAsync(Order order);
    Task UpdateAsync(Order order);
    Task<bool> DeleteAsync(Guid orderId);
}