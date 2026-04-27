using Bitstore.Core.Enums;

namespace Bitstore.Core.Models;

public class License
{
    private readonly List<OrderItem> _orderItems = new();

    private License(LicenseType type, string name,  decimal price, Beat beat )
    {
        Id =  Guid.NewGuid();
        Name = name;
        Type = type;
        Price = price;
        Beat = beat;
        BeatId = beat.Id;
    }

    private License(Guid id, LicenseType type, decimal price, string name, 
        Guid beatId, Beat? beat = null, List<OrderItem>? orderItems = null)
    {
        Id = id;
        Type = type;
        Price = price;
        Name = name;
        BeatId = beatId;
        Beat = beat;
        
        if (orderItems != null)
        {
            _orderItems = new List<OrderItem>(orderItems);
        }
    }
    
    public Guid Id { get; private set; }
    public LicenseType Type { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public Guid BeatId { get;  private set;}
    public Beat? Beat { get;  private set;}
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;
    
    public void AddItem(OrderItem item)
    {
        if (item == null) 
            throw new ArgumentNullException(nameof(item));
        _orderItems.Add(item);
    }

    public static License Create(string name, decimal price, LicenseType type, Beat beat)
    {
        if (beat == null)
            throw new ArgumentNullException(nameof(beat));
        
        if (beat.Id == Guid.Empty)
            throw new ArgumentException("Beat must have a valid id", nameof(beat));
        
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));
        
        var license = new License(type, name, price, beat);
        
        return license;
    }
    
    public static License FromEntity(Guid id, LicenseType type, decimal price, 
        string name, Guid beatId, Beat? beat = null, List<OrderItem>? orderItems = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty", nameof(id));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));
        
        if (beatId == Guid.Empty)
            throw new ArgumentException("BeatId cannot be empty", nameof(beatId));
        
        return new License(id, type, price, name, beatId, beat, orderItems);
    }
}