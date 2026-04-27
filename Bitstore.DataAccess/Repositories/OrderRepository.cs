using Bitstore.Application.Exceptions;
using Bitstore.Core.Abstractions;
using Bitstore.Core.Models;
using Bitstore.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Bitstore.DataAccess.Repositories;

public class OrderRepository(BitstoreDbContext context) : IOrderRepository
{
    private readonly BitstoreDbContext _context = context;

    public async Task<List<Order>> GetAllAsync()
    {
        Log.Information("Getting all orders");

        var orderEntities = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Buyer)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        if (!orderEntities.Any())
        {
            Log.Information("No orders found");
            return new List<Order>();
        }

        var orders = orderEntities.Select(orderEntity => Order.FromEntity(
            orderEntity.Id,
            orderEntity.OrderNumber,
            orderEntity.BuyerId,
            orderEntity.TotalAmount,
            orderEntity.Status,
            orderEntity.CreatedAt,
            new List<OrderItem>()  
        )).ToList();

        Log.Information("Returned {Count} orders", orders.Count);
        return orders;
    }

    public async Task<Order> GetByIdAsync(Guid orderId)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty", nameof(orderId));
    
        Log.Information("Getting order by id {OrderId}", orderId);

        var orderEntity = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Buyer)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (orderEntity == null)
        {
            Log.Warning("Order {OrderId} not found", orderId);
            throw new NotFoundException($"Order with id {orderId} not found");
        }
    
        var order = Order.FromEntity(
            orderEntity.Id,
            orderEntity.OrderNumber,
            orderEntity.BuyerId,
            orderEntity.TotalAmount,
            orderEntity.Status,
            orderEntity.CreatedAt,
            new List<OrderItem>() 
        );

        Log.Information("Order {OrderId} retrieved successfully", orderId);
        return order;
    }

    public async Task<Order> GetByIdWithDetailsAsync(Guid orderId)
{
    if (orderId == Guid.Empty)
        throw new ArgumentException("OrderId cannot be empty", nameof(orderId));
    
    Log.Information("Getting order by id with details {OrderId}", orderId);

    try
    {
        var orderEntity = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Buyer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Beat)
            .Include(o => o.Items)
                .ThenInclude(i => i.License)
            .Include(o => o.Items)
                .ThenInclude(i => i.Seller)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (orderEntity == null)
        {
            Log.Warning("Order {OrderId} not found", orderId);
            throw new NotFoundException($"Order with id {orderId} not found");
        }
        
        var order = Order.FromEntity(
            orderEntity.Id,
            orderEntity.OrderNumber,
            orderEntity.BuyerId,
            orderEntity.TotalAmount,
            orderEntity.Status,
            orderEntity.CreatedAt,
            new List<OrderItem>() 
        );
        
        var orderItems = orderEntity.Items.Select(item => 
        {
           
            var beat = Beat.FromEntity(
                item.Beat.Id,
                item.Beat.Title,
                item.Beat.Price,
                item.Beat.AudioUrl,
                item.Beat.IsPublished,
                item.Beat.Description,
                item.Beat.CoverUrl,
                item.Beat.UserId,
                item.Beat.CreatedAt
            );
            
            var license = License.FromEntity(
                item.License.Id,
                item.License.Type,
                item.License.Price,
                item.License.Name,
                item.License.BeatId
            );
            
            var seller = User.FromEntity(
                item.Seller.Id,
                item.Seller.Username,
                item.Seller.Email,
                item.Seller.Role
            );
            
            
            return OrderItem.FromEntityWithNavigation(
                item.Id,
                item.Price,
                order, 
                beat,
                license,
                seller
            );
        }).ToList();

        order.SetItems(orderItems);

        Log.Information("Order {OrderId} retrieved successfully", orderId);
        return order;
    }
    catch (Exception ex) when (ex is not NotFoundException)
    {
        Log.Error(ex, "Error getting order {OrderId}", orderId);
        throw;
    }
}

    public async Task<List<Order>> GetByBuyerIdAsync(Guid buyerId)
{
    if (buyerId == Guid.Empty)
        throw new ArgumentException("BuyerId cannot be empty", nameof(buyerId));
    
    Log.Information("Getting orders by buyer id {BuyerId}", buyerId);

    try
    {
        var orderEntities = await _context.Orders
            .AsNoTracking()
            .Where(o => o.BuyerId == buyerId) 
            .Include(o => o.Buyer)              
            .Include(o => o.Items)              
                .ThenInclude(i => i.Beat)       
            .Include(o => o.Items)
                .ThenInclude(i => i.License)    
            .OrderByDescending(o => o.CreatedAt) 
            .ToListAsync();

        if (!orderEntities.Any())
        {
            Log.Information("No orders found for buyer {BuyerId}", buyerId);
            return new List<Order>();
        }

        var orders = orderEntities.Select(orderEntity =>
        {
            var orderItems = orderEntity.Items
                .Select(item => OrderItem.FromEntity(
                    item.Id,
                    item.Price,
                    item.OrderId,
                    item.BeatId,
                    item.LicenseId,
                    item.SellerId
                ))
                .ToList();
            
            return Order.FromEntity(
                orderEntity.Id,
                orderEntity.OrderNumber,
                orderEntity.BuyerId,
                orderEntity.TotalAmount,
                orderEntity.Status,
                orderEntity.CreatedAt,
                orderItems
            );
        }).ToList();

        Log.Information("Returned {Count} orders for buyer {BuyerId}", orders.Count, buyerId);
        return orders;
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error getting orders for buyer {BuyerId}", buyerId);
        throw;
    }
}

    public async Task<List<Order>> GetBySellerIdAsync(Guid sellerId)
{
    if (sellerId == Guid.Empty)
        throw new ArgumentException("SellerId cannot be empty", nameof(sellerId));
    
    Log.Information("Getting orders by seller id {SellerId}", sellerId);

    try
    {
        var orderEntities = await _context.Orders
            .AsNoTracking()
            .Where(o => o.Items.Any(i => i.SellerId == sellerId))
            .Include(o => o.Buyer)
            .Include(o => o.Items.Where(i => i.SellerId == sellerId))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        if (!orderEntities.Any())
        {
            Log.Information("No orders found for seller {SellerId}", sellerId);
            return new List<Order>();
        }
        
        var orders = orderEntities.Select(orderEntity => 
        {
            var orderItems = orderEntity.Items
                .Select(item => OrderItem.FromEntity(
                    item.Id,
                    item.Price,
                    item.OrderId,
                    item.BeatId,
                    item.LicenseId,
                    item.SellerId
                ))
                .ToList();
            
            return Order.FromEntity(
                orderEntity.Id,
                orderEntity.OrderNumber,
                orderEntity.BuyerId,
                orderEntity.TotalAmount,
                orderEntity.Status,
                orderEntity.CreatedAt,
                orderItems
            );
        }).ToList();

        Log.Information("Returned {Count} orders for seller {SellerId}", orders.Count, sellerId);

        return orders;
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error getting orders for seller {SellerId}", sellerId);
        throw;
    }
}

    public async Task CreateAsync(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        Log.Information("Creating order for buyer {BuyerId}", order.BuyerId);

        var existingLicense = await _context.Orders
            .AnyAsync(o => o.Id == order.Id);
        
        if (existingLicense)
            throw new AlreadyExistException($"Order {order.Id} already exists");
        
        var orderEntity = new OrderEntity
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            BuyerId = order.BuyerId,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt
        };

        await _context.Orders.AddAsync(orderEntity);
        await _context.SaveChangesAsync();

        Log.Information("Created order {OrderId} for buyer {BuyerId}", order.Id, order.BuyerId);
    }

    public async Task UpdateAsync(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (order.Id == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty", nameof(order.Id));

        Log.Information("Updating order {OrderId}", order.Id);

        var orderExists = await _context.Orders
            .AnyAsync(o => o.Id == order.Id);

        if (!orderExists)
            throw new NotFoundException($"Order {order.Id} not found");

        await _context.Orders
            .Where(o => o.Id == order.Id)
            .ExecuteUpdateAsync(o => o
                .SetProperty(x => x.TotalAmount, order.TotalAmount)
                .SetProperty(x => x.Status, order.Status));

        Log.Information("Updated order {OrderId}", order.Id);
    }

    public async Task<bool> DeleteAsync(Guid orderId)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty", nameof(orderId));

        Log.Information("Deleting order {OrderId}", orderId);

        var orderExists = await _context.Orders
            .AnyAsync(o => o.Id == orderId);

        if (!orderExists)
            return false;

        var deletedCount = await _context.Orders
            .Where(o => o.Id == orderId)
            .ExecuteDeleteAsync();

        Log.Information("Deleted order {OrderId}: {Result}", orderId, deletedCount > 0);

        return deletedCount > 0;
    }
}