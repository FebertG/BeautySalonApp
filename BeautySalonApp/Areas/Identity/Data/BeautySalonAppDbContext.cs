using BeautySalonApp.Areas.Identity.Data;
using BeautySalonApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BeautySalonApp.Data;

public class BeautySalonAppDbContext : IdentityDbContext<UserApp>
{
    public BeautySalonAppDbContext(DbContextOptions<BeautySalonAppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Address> Address { get; set; }
    public DbSet<Employee> Employee { get; set; }
    public DbSet<EmployeesWorkingDate> EmployeesWorkingDate { get; set; }
    public DbSet<GiftCard> GiftCard { get; set; }
    public DbSet<Opinion> Opinion { get; set; }
    public DbSet<Product> Product { get; set; }
    public DbSet<Reservation> Reservation { get; set; }
    public DbSet<Salon> Salon { get; set; }
    public DbSet<Service> Service { get; set; }
    public DbSet<Warehouse> Warehouse { get; set; }



    /*protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserApp>(entity =>
        {
            entity.HasOne(u => u.Salon)
            .WithOne(p => p.UserApp)
                .HasForeignKey<Salon>(p => p.Id);

            entity.Navigation(p => p.Salon).AutoInclude();
        });
    }*/
}
