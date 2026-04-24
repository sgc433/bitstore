namespace Bitstore.Core.Models;

public class Beat
{
    private readonly List<License> _licenses = new();
    private readonly List<OrderItem> _orderItems = new();
    
    // Constructor for creating a beat
    private Beat(string title, decimal price, string audioUrl, bool isPublished, 
        string? description, string? coverUrl, User user)
    {
        Id = Guid.NewGuid();
        Title = title;
        Price = price;
        AudioUrl = audioUrl;
        CoverUrl = coverUrl;
        Description = description;
        IsPublished = isPublished;
        UserId = user.Id;
        User = user;
        CreatedAt = DateTime.UtcNow;
    }
    
    // Constructor for transformation from Entity to Class
    private Beat(Guid id, string title, decimal price, string audioUrl, bool isPublished, 
        string? description, string? coverUrl, Guid userId, DateTime createdAt)
    {
        Id = id;
        Title = title;
        Price = price;
        AudioUrl = audioUrl;
        CoverUrl = coverUrl;
        Description = description;
        IsPublished = isPublished;
        UserId = userId;
        CreatedAt = createdAt;
    }
    
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public decimal Price  { get; private set;}
    public string? Description { get; private set;}
    public string AudioUrl { get; private set;}
    public string? CoverUrl { get; private set;} //обложка
    public bool IsPublished { get; private set;}
    public DateTime CreatedAt { get; }
    public Guid UserId { get; }
    public User? User { get; }
    
    public IReadOnlyCollection<License> Licenses =>  _licenses;
    public IReadOnlyCollection<OrderItem> OrderItems =>  _orderItems;
    
    
    public static Beat Create(string title, decimal price, string audioUrl, bool isPublished, 
        string? description, string? coverUrl, User user)
    {
        //there will be a validation
        if (string.IsNullOrEmpty(title) ||  string.IsNullOrEmpty(audioUrl))
            throw new Exception();
        if (user.Id ==  Guid.Empty)
            throw new ArgumentNullException();
            
        var beat = new Beat(title, price, audioUrl, isPublished, 
            description, coverUrl, user);
        
        return beat;
    }

    public static Beat FromEntity(Guid id, string title, decimal price, string audioUrl, bool isPublished, 
        string? description, string? coverUrl, Guid userId, DateTime createdAt)
    {
        if (string.IsNullOrEmpty(title) ||  string.IsNullOrEmpty(audioUrl))
            throw new Exception();
            
        var beat = new Beat(id, title, price, audioUrl, isPublished,
            description, coverUrl, userId, createdAt);
        
        return beat;
    }
    
    public void Update(string title, decimal price, string audioUrl, bool isPublished,
        string? description, string? coverUrl)
    {
        if (string.IsNullOrEmpty(title))
            throw new ArgumentException("Title cannot be empty");
        
        if (string.IsNullOrEmpty(audioUrl))
            throw new ArgumentException("AudioUrl cannot be empty");
        
        if (price < 0)
            throw new ArgumentException("Price cannot be negative");
        
        Title = title;
        Price = price;
        AudioUrl = audioUrl;
        IsPublished = isPublished;
        Description = description;
        CoverUrl = coverUrl;
        
    }
    
    public void AddLicense(License license)
    {
        if (license == null)
            throw new ArgumentNullException();
        
        _licenses.Add(license);
    }

    public void AddOrderItem(OrderItem orderItem)
    {
        if (orderItem == null)
            throw new ArgumentNullException();
        _orderItems.Add(orderItem);
    }
}