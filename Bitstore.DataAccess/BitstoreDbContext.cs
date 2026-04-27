using Bitstore.DataAccess.Configuration;
using Bitstore.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bitstore.DataAccess;

public class BitstoreDbContext(DbContextOptions<BitstoreDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<BeatEntity> Beats { get; set; }
    public DbSet<OrderEntity> Orders { get; set; }
    public DbSet<LicenseEntity> Licenses { get; set; }
    public DbSet<OrderItemEntity> OrderItems { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new BeatConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new LicenseConfiguration());
    }
}