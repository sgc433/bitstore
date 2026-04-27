namespace Bitstore.Core.Models;

public class User
{
    private readonly List<Beat> _beats = new();
    private readonly List<Order> _orders = new();
    private readonly List<OrderItem> _orderItems = new();
    private User(Guid userId, string username,
        string email, string passwordHash, string role)
    {
        Id = userId;
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }

    private User(Guid userId, string username,
        string email, string role)
    {
        Id = userId;
        Username = username;
        Email = email;
        Role = role;
        PasswordHash = "";
    }
    
    public Guid  Id { get; private set; }
    public string Username { get;  private set; }
    public string Email { get; private set; }
    public string Role { get; private set; } 
    public string PasswordHash { get;  private set; }
    public decimal Balance { get; private set; } = 0;
    
    public IReadOnlyCollection<Beat> Beats => _beats;
    public IReadOnlyCollection<Order> Orders => _orders;
    public IReadOnlyCollection<OrderItem> SoldItems => _orderItems;
    
    public static  User Create(Guid userId, string username, 
        string email, string passwordHash, string role)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(passwordHash))
            throw new ArgumentNullException();
        
        return new User(userId, username, email, passwordHash, role);
    }

    public static User FromEntity(Guid id, string username, string email, string role)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty", nameof(id));
    
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));
    
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));
        
        return new User(id, username, email, role);
    }
    
    public void AddBeat(Beat beat)
    {
        if (beat == null)
            throw new ArgumentNullException();
        _beats.Add(beat);
    }

    public void AddOrder(Order order)
    {
        if (order == null)
            throw new ArgumentNullException();
        _orders.Add(order);
    }

    public void AddOrderItem(OrderItem orderItem)
    {
        if (orderItem == null)
            throw new ArgumentNullException();
        _orderItems.Add(orderItem);
    }
}