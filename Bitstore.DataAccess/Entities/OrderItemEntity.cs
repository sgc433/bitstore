namespace Bitstore.DataAccess.Entities;

public class OrderItemEntity
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
    
    public Guid OrderId { get; set; }
    public OrderEntity Order { get; set; }
    
    public Guid BeatId { get; set; }
    public BeatEntity Beat { get; set; }
    
    public Guid LicenseId { get; set; }
    public LicenseEntity License { get; set; }
    
    public Guid SellerId { get; set; }
    public UserEntity Seller { get; set; } 
}