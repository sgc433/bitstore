using Bitstore.Application.Abstractions;
using Bitstore.Application.DTO.Order;
using Bitstore.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bitstore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController(IOrderService orderService) : Controller
{
    private readonly IOrderService _orderService = orderService;

    [Authorize]
    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderResponse>> GetOrderById(Guid orderId)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("Order id cannot be empty");

        var order = await _orderService.GetOrderById(orderId);
        return Ok(order);
    }

    [Authorize]
    [HttpGet("buyer/{buyerId}")]
    public async Task<ActionResult<List<OrderResponse>>> GetOrdersByBuyer(Guid buyerId)
    {
        if (buyerId == Guid.Empty)
            throw new ArgumentException("Buyer id cannot be empty");

        var orders = await _orderService.GetOrdersByBuyer(buyerId);
        return Ok(orders);
    }

    [Authorize(Roles = "Admin,Producer")]
    [HttpGet("seller/{sellerId}")]
    public async Task<ActionResult<List<OrderResponse>>> GetOrdersBySeller(Guid sellerId)
    {
        if (sellerId == Guid.Empty)
            throw new ArgumentException("Seller id cannot be empty");

        var orders = await _orderService.GetOrdersBySeller(sellerId);
        return Ok(orders);
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder(OrderRequest request)
    {
        var order = await _orderService.CreateOrder(request);
        return Ok(order);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{orderId}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid orderId, OrderStatus status)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("Order id cannot be empty");

        var order = await _orderService.UpdateOrderStatus(orderId, status);
        return Ok(order);
    }

    [Authorize]
    [HttpDelete("{orderId}/cancel")]
    public async Task<ActionResult<bool>> CancelOrder(Guid orderId)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("Order id cannot be empty");

        var result = await _orderService.CancelOrder(orderId);
        return Ok(result);
    }
}