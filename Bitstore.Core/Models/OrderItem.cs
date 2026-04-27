namespace Bitstore.Core.Models;

public class OrderItem
{
    private OrderItem(decimal price, Order order, Beat beat,
        License license, User seller)
    {
        Id =  Guid.NewGuid();
        Price = price;
        Order = order;
        OrderId = order.Id;
        BeatId = beat.Id;
        Beat = beat;
        License = license;
        LicenseId = license.Id;
        Seller = seller;
        SellerId = seller.Id;
    }
    
    private OrderItem(Guid id, decimal price, Guid orderId, Guid beatId, 
        Guid licenseId, Guid sellerId)
    {
        Id = id;
        Price = price;
        OrderId = orderId;
        BeatId = beatId;
        LicenseId = licenseId;
        SellerId = sellerId;
        
        Order = null!;
        Beat = null!;
        License = null!;
        Seller = null!;
    }
    
    public Guid Id { get;  }
    public decimal Price { get;  private set; }
    
    public Guid OrderId { get;  private set; }
    public Order? Order { get; private set; }
    
    public Guid BeatId { get; private set;  }
    public Beat? Beat { get; private set; }
    
    public Guid LicenseId { get; private set;  }
    public License? License { get; private set; }
    
    public Guid SellerId { get; private set;  }
    public User? Seller { get; private set; }
    
    public static OrderItem Create(decimal price, Order order, Beat beat, 
        License license, User seller)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));
        
        if (order == null)
            throw new ArgumentNullException(nameof(order));
        
        if (beat == null)
            throw new ArgumentNullException(nameof(beat));
        
        if (license == null)
            throw new ArgumentNullException(nameof(license));
        
        if (seller == null)
            throw new ArgumentNullException(nameof(seller));
        
        return new OrderItem(price, order, beat, license, seller);
    }
    
    public static OrderItem FromEntity(Guid id, decimal price, Guid orderId, 
        Guid beatId, Guid licenseId, Guid sellerId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty", nameof(id));
        
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));
        
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty", nameof(orderId));
        
        if (beatId == Guid.Empty)
            throw new ArgumentException("BeatId cannot be empty", nameof(beatId));
        
        if (licenseId == Guid.Empty)
            throw new ArgumentException("LicenseId cannot be empty", nameof(licenseId));
        
        if (sellerId == Guid.Empty)
            throw new ArgumentException("SellerId cannot be empty", nameof(sellerId));
        
        return new OrderItem(id, price, orderId, beatId, licenseId, sellerId);
    }
    
    public static OrderItem FromEntityWithNavigation(
        Guid id, 
        decimal price, 
        Order order,
        Beat beat, 
        License license, 
        User seller)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty", nameof(id));
        
        if (beat == null)
            throw new ArgumentNullException(nameof(beat));
    
        if (license == null)
            throw new ArgumentNullException(nameof(license));
    
        if (seller == null)
            throw new ArgumentNullException(nameof(seller));
    
        var orderItem = FromEntity(id, price, order.Id, beat.Id, license.Id, seller.Id);
    
        typeof(OrderItem).GetProperty(nameof(Order))?.SetValue(orderItem, order);
        typeof(OrderItem).GetProperty(nameof(Beat))?.SetValue(orderItem, beat);
        typeof(OrderItem).GetProperty(nameof(License))?.SetValue(orderItem, license);
        typeof(OrderItem).GetProperty(nameof(Seller))?.SetValue(orderItem, seller);
    
        return orderItem;
    }

    public void SetOrder(Order order)
    {
        if (order == null)
            throw new  ArgumentNullException(nameof(order));
        
        Order = order;
        OrderId = order.Id;
    }
}