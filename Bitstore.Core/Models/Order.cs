using Bitstore.Core.Enums;

namespace Bitstore.Core.Models;

public class Order
{
    private readonly List<OrderItem> _items = new();
    private Order(User user, decimal totalAmount)
    {
        Id = Guid.NewGuid();
        OrderNumber = GenerateOrderNumber();
        Buyer = user;
        BuyerId = user.Id;
        CreatedAt = DateTime.UtcNow;
        TotalAmount = totalAmount;
        Status = OrderStatus.Pending;
    }
    
    private Order(Guid id, string orderNumber, Guid buyerId, decimal totalAmount, 
        OrderStatus status, DateTime createdAt, List<OrderItem>? items)
    {
        Id = id;
        OrderNumber = orderNumber;
        BuyerId = buyerId;
        TotalAmount = totalAmount;
        Status = status;
        CreatedAt = createdAt;
        
        if (items != null)
            _items = new List<OrderItem>(items);
    }
    
    public Guid Id { get;  private set; }
    public string OrderNumber { get; private set; }
    public Guid BuyerId { get; private set; }
    public User? Buyer { get;  private set;}
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get;  private set;} 
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items;

    public void AddItem(OrderItem item)
    {
        if (item == null)
            throw new ArgumentNullException();
        _items.Add(item);
    }

    public void RemoveItem(Guid itemId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot remove items from non-pending order");
        
        var item = _items.FirstOrDefault(x => x.Id == itemId);
        
        if (item != null)
            _items.Remove(item);
    }
    
    public static Order Create(User buyer, decimal totalAmount)
    {
        if (buyer == null)
            throw new ArgumentNullException(nameof(buyer));
        
        if (buyer.Id == Guid.Empty)
            throw new ArgumentException("Buyer must be a valid user", nameof(buyer));
        
        if (totalAmount < 0)
            throw new ArgumentException("Total amount cannot be negative", nameof(totalAmount));
        
        var order = new Order(buyer, totalAmount);
        
        return order;
    }

    public static Order FromEntity(Guid id, string orderNumber, Guid buyerId, 
        decimal totalAmount, OrderStatus status, DateTime createdAt, List<OrderItem> items)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty", nameof(id));
        
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number cannot be empty", nameof(orderNumber));
        
        if (buyerId == Guid.Empty)
            throw new ArgumentException("BuyerId cannot be empty", nameof(buyerId));
        
        if (totalAmount < 0)
            throw new ArgumentException("Total amount cannot be negative", nameof(totalAmount));
        
        if (createdAt == default)
            throw new ArgumentException("CreatedAt cannot be default", nameof(createdAt));
        
        items ??= new List<OrderItem>();
        
        var order = new Order(id, orderNumber, buyerId, totalAmount, status, createdAt, items);
        
        return order;
    }

    public void SetItems(List<OrderItem> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));
        
        _items.Clear();
        _items.AddRange(items);
    }
    
    
    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
    }
}