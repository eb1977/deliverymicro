using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggegate;
using DeliveryApp.Infrastructure.Adapters.PostgeSQL.Entities;
using DeliveryApp.Infrastructure.Adapters.PostgeSQL.EntityConfigurations;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.PostgeSQL;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }

    public DbSet<Courier> Couriers { get; set; }

    public DbSet<Assignment> Assignments { get; set; }

    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply Configuration
        modelBuilder.ApplyConfiguration(new OrderEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new CourierEntityConfiguration());
        modelBuilder.ApplyConfiguration(new AssignmentEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageEntityTypeConfiguration());

        //// Seed
        //modelBuilder.Entity<DeliveryPeriod>(b =>
        //{
        //    var allDeliveryPeriods = DeliveryPeriod.List();
        //    b.HasData(allDeliveryPeriods.Select(c => new { c.Id, c.Name, c.From, c.To }));
        //});

        //modelBuilder.Entity<Good>(b =>
        //{
        //    var allGoods = Good.List().ToList();
        //    b.HasData(allGoods.Select(c => new { c.Id, c.Title, c.Description, c.PurchaseCount }));
        //    b.OwnsOne(e => e.Weight).HasData(allGoods.Select(c => new { GoodId = c.Id, c.Weight.Value }));
        //    b.OwnsOne(e => e.Quantity).HasData(allGoods.Select(c => new { GoodId = c.Id, c.Quantity.Value }));
        //    b.OwnsOne(e => e.Price).HasData(allGoods.Select(c => new { GoodId = c.Id, c.Price.Value }));
        //});
    }
}
