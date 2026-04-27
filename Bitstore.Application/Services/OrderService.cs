using Bitstore.Application.Abstractions;
using Bitstore.Application.DTO.Order;
using Bitstore.Application.DTO.OrderItem;
using Bitstore.Application.Exceptions;
using Bitstore.Core.Abstractions;
using Bitstore.Core.Enums;
using Bitstore.Core.Models;
using Serilog;

namespace Bitstore.Application.Services;

public class OrderService(
    IOrderRepository orderRepository,
    ILicenseRepository licenseRepository,
    IBeatRepository beatRepository,
    IUserRepository userRepository,
    ICurrentUserService currentUserService) : IOrderService
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly ILicenseRepository _licenseRepository = licenseRepository;
    private readonly IBeatRepository _beatRepository = beatRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<OrderResponse> GetOrderById(Guid orderId)
    {
        Log.Information("Getting order by id {OrderId}", orderId);
        try
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);

            if (order == null)
                throw new NotFoundException($"Order with id {orderId} not found");

            var currentUserId = _currentUserService.GetUserId();
            var currentUserRole = _currentUserService.GetUserRole();

            if (order.BuyerId != currentUserId && currentUserRole != "Admin")
            {
                Log.Warning("User {UserId} attempted to access order {OrderId} owned by {OwnerId}", 
                    currentUserId, orderId, order.BuyerId);
                throw new UnauthorizedAccessException("You can only view your own orders");
            }

            var response = new OrderResponse(
                order.Id,
                order.OrderNumber,
                order.BuyerId,
                order.TotalAmount,
                order.Status,
                order.CreatedAt,
                new List<OrderItemResponse>()
            );
            
            Log.Information("Order {OrderId} retrieved successfully by user {UserId}", 
                orderId, currentUserId);
        
            return response;
        }
        catch (Exception ex) when (ex is not NotFoundException && ex is not UnauthorizedAccessException)
        {
            Log.Error(ex, "Error getting order {OrderId}", orderId);
            throw;
        }
    }
    public async Task<List<OrderResponse>> GetOrdersByBuyer(Guid buyerId)
    {
        var currentUserId = _currentUserService.GetUserId();
        var currentUserRole = _currentUserService.GetUserRole();

        if (currentUserId != buyerId && currentUserRole != "Admin")
        {
            Log.Warning("User {UserId} attempted to access orders of user {BuyerId}", 
                currentUserId, buyerId);
            throw new UnauthorizedAccessException("You can only view your own orders");
        }

        Log.Information("Getting orders for buyer {BuyerId}", buyerId);

        try
        {
            var buyer = await _userRepository.GetByIdAsync(buyerId);
            if (buyer == null)
                throw new NotFoundException($"User with id {buyerId} not found");
        
            var orders = await _orderRepository.GetByBuyerIdAsync(buyerId);
        
            var response = orders.Select(o => new OrderResponse(
                o.Id,
                o.OrderNumber,
                o.BuyerId,
                o.TotalAmount,
                o.Status,
                o.CreatedAt,
                new List<OrderItemResponse>())
            ).ToList();
        
            Log.Information("Returned {Count} orders for buyer {BuyerId}", response.Count, buyerId);
        
            return response;
        }
        catch (Exception ex) when (ex is not UnauthorizedAccessException && ex is not NotFoundException)
        {
            Log.Error(ex, "Error getting orders for buyer {BuyerId}", buyerId);
            throw;
        }
    }

    public async Task<List<OrderResponse>> GetOrdersBySeller(Guid sellerId)
{
    var currentUserId = _currentUserService.GetUserId();
    var currentUserRole = _currentUserService.GetUserRole();

    Log.Information("User {UserId} requesting orders for seller {SellerId}", 
        currentUserId, sellerId);

    if (currentUserId != sellerId && currentUserRole != "Admin")
    {
        Log.Warning("User {UserId} attempted to access orders of seller {SellerId}", 
            currentUserId, sellerId);
        throw new UnauthorizedAccessException("You can only view your own orders as a seller");
    }

    try
    {
        var seller = await _userRepository.GetByIdAsync(sellerId);
        if (seller == null)
            throw new NotFoundException($"User with id {sellerId} not found");
        
        if (currentUserRole != "Admin" && currentUserId !=  sellerId)
        {
            Log.Warning("User {SellerId} has no beats and cannot be a seller", sellerId);
            return new List<OrderResponse>();
        }
        
        var orders = await _orderRepository.GetBySellerIdAsync(sellerId);
        
        var response = orders.Select(o => new OrderResponse(
            o.Id,
            o.OrderNumber,
            o.BuyerId,
            o.TotalAmount,
            o.Status,
            o.CreatedAt,
            new List<OrderItemResponse>())
        ).ToList();
        
        Log.Information("Returned {Count} orders for seller {SellerId}", response.Count, sellerId);
        
        return response;
    }
    catch (Exception ex) when (ex is not UnauthorizedAccessException && ex is not NotFoundException)
    {
        Log.Error(ex, "Error getting orders for seller {SellerId}", sellerId);
        throw;
    }
}

    public async Task<OrderResponse> CreateOrder(OrderRequest request)
{
    if (request.Items == null || !request.Items.Any())
        throw new ArgumentException("Order must contain at least one item", nameof(request));
    
    var currentUserId = _currentUserService.GetUserId();
    if (currentUserId == Guid.Empty)
        throw new UnauthorizedAccessException("User is not authenticated");
    
    Log.Information("User {UserId} is creating a new order with {ItemCount} items", 
        currentUserId, request.Items.Count);

    try
    {
        var buyer = await _userRepository.GetByIdAsync(currentUserId);
        if (buyer == null)
            throw new NotFoundException($"User {currentUserId} not found");
        
        var orderItems = new List<OrderItem>();
        var licensesToUpdate = new List<License>();
        var totalAmount = 0m;
        
        foreach (var itemRequest in request.Items)
        {
            var license = await _licenseRepository.GetByIdWithDetailsAsync(itemRequest.LicenseId);
            if (license == null)
                throw new NotFoundException($"License with id {itemRequest.LicenseId} not found");
            
            var beat = await _beatRepository.GetByIdAsync(itemRequest.BeatId);
            if (beat == null)
                throw new NotFoundException($"Beat with id {itemRequest.BeatId} not found");
            
            if (license.BeatId != beat.Id)
                throw new InvalidOperationException($"License {license.Id} does not belong to beat {beat.Id}");
            
            if (beat.UserId == currentUserId)
                throw new InvalidOperationException("You cannot purchase your own beat");
            
            var seller = await _userRepository.GetByIdAsync(beat.UserId);
            if (seller == null)
                throw new NotFoundException($"Seller {beat.UserId} not found");
            
            totalAmount += license.Price;
            
            var orderItem = OrderItem.Create(
                license.Price,
                null!, 
                beat,
                license,
                seller
            );
            orderItems.Add(orderItem);
            
            licensesToUpdate.Add(license);
        }
        
        var order = Order.Create(buyer, totalAmount);
        order.SetItems(orderItems);
        
        foreach (var item in orderItems)
        {
            item.SetOrder(order);
        }
        
        await _orderRepository.CreateOrderWithItemsAsync(order, licensesToUpdate);
        
        Log.Information("Order {OrderId} created successfully for user {UserId} with total {TotalAmount:C}", 
            order.Id, currentUserId, totalAmount);
        
        return new OrderResponse(
            order.Id,
            order.OrderNumber,
            order.BuyerId,
            order.TotalAmount,
            order.Status,
            order.CreatedAt,
            order.Items.Select(item =>
            {
                if (item.Beat != null && item.License != null && item.Seller != null)
                    return new OrderItemResponse(
                        item.Id,
                        item.Price,
                        item.BeatId,
                        item.Beat.Title,
                        item.LicenseId,
                        item.License.Type,
                        item.SellerId,
                        item.Seller.Username
                    );
                throw new InvalidOperationException();
            }).ToList()
        );
    }
    catch (Exception ex) when (ex is not NotFoundException && ex is not UnauthorizedAccessException)
    {
        Log.Error(ex, "Error creating order for user {UserId}", currentUserId);
        throw new InvalidOperationException("Failed to create order. Please try again.", ex);
    }
}

    public async Task<OrderResponse> UpdateOrderStatus(Guid orderId, OrderStatus status)
{
    var currentUserId = _currentUserService.GetUserId();
    var currentUserRole = _currentUserService.GetUserRole();

    Log.Information("User {UserId} attempting to update order {OrderId} status to {Status}", 
        currentUserId, orderId, status);

    var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);

    if (order == null)
        throw new NotFoundException($"Order with id {orderId} not found");

    if (currentUserRole != "Admin")
    {
        Log.Warning("User {UserId} does not have permission to update order status", currentUserId);
        throw new UnauthorizedAccessException("Only admin users can update order status");
    }

    try
    {
        switch (status)
        {
            case OrderStatus.Confirmed:
                order.Confirm();
                break;
            case OrderStatus.Completed:
                order.Complete();
                break;
            case OrderStatus.Cancelled:
                order.Cancel();
                break;
            case OrderStatus.Pending:
                throw new InvalidOperationException("Cannot set status back to Pending");
            default:
                throw new ArgumentOutOfRangeException(nameof(status), $"Unsupported status: {status}");
        }
        
        await _orderRepository.UpdateAsync(order);
        
        Log.Information("Order {OrderId} status updated from {OldStatus} to {NewStatus}", 
            orderId, order.Status, status);
        
        var response = new OrderResponse(
            order.Id,
            order.OrderNumber,
            order.BuyerId,
            order.TotalAmount,
            order.Status,  
            order.CreatedAt,
            order.Items.Select(item => new OrderItemResponse(
                item.Id,
                item.Price,
                item.BeatId,
                item.Beat.Title,
                item.LicenseId,
                item.License.Type,
                item.SellerId,
                item.Seller.Username
            )).ToList()
        );
        
        return response;
    }
    catch (InvalidOperationException ex)
    {
        Log.Warning(ex, "Invalid status transition for order {OrderId} to {Status}", orderId, status);
        throw;
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error updating order {OrderId} status to {Status}", orderId, status);
        throw;
    }
}

    public async Task<bool> CancelOrder(Guid orderId)
    {
        var currentUserId = _currentUserService.GetUserId();
        var currentUserRole = _currentUserService.GetUserRole();

        Log.Information("User {UserId} attempting to cancel order {OrderId}", currentUserId, orderId);

        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
            throw new NotFoundException($"Order with id {orderId} not found");

        if (order.BuyerId != currentUserId && currentUserRole != "Admin")
        {
            Log.Warning("User {UserId} does not have permission to cancel order {OrderId}", currentUserId, orderId);
            throw new UnauthorizedAccessException("You can only cancel your own orders");
        }

        if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
        {
            Log.Warning("Order {OrderId} cannot be cancelled due to status {Status}", orderId, order.Status);
            throw new InvalidOperationException($"Order cannot be cancelled. Current status: {order.Status}");
        }

        var result = await _orderRepository.DeleteAsync(orderId);

        Log.Information("Order {OrderId} cancelled: {Result}", orderId, result);

        return result;
    }

    private OrderResponse MapToOrderResponse(Order order)
    {
        return new OrderResponse(
            order.Id,
            order.OrderNumber,
            order.BuyerId,
            order.TotalAmount,
            order.Status,
            order.CreatedAt,
            new List<OrderItemResponse>()
        );
    }
}