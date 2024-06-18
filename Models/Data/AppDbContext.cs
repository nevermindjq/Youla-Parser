using Microsoft.EntityFrameworkCore;

using Models.Net;

namespace Models.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) {
    // Bot
    public DbSet<User> Users { get; set; }
    public DbSet<ApproveMessage> ApproveMessages { get; set; }
    
    // Youla
    public DbSet<Category> Categories { get; set; }
    public DbSet<City.City> Cities { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Seller> Sellers { get; set; }
    
    // Http
    public DbSet<Cookie> Cookies { get; set; }
    public DbSet<Proxy> Proxies { get; set; }

    protected override void OnModelCreating(ModelBuilder builder) {
        builder.Entity<City.City>()
            .HasKey(x => x.Name);

        builder.Entity<Product>(entity =>
        {
            entity.HasKey(x => x.Id);
            
            entity.HasOne<Seller>(x => x.Seller)
                .WithMany(x => x.Products)
                .HasPrincipalKey(x => x.Id)
                .HasForeignKey(x => x.SellerId);

            entity.HasOne<Category>(x => x.Category)
                .WithMany(x => x.Products)
                .HasPrincipalKey(x => x.Id)
                .HasForeignKey(x => x.CategoryId);
        });

        builder.Entity<Seller>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasMany<Product>(x => x.Products)
                .WithOne(x => x.Seller)
                .HasPrincipalKey(x => x.Id)
                .HasForeignKey(x => x.SellerId);
        });

        builder.Entity<Category>(entity =>
        {
            entity.HasKey(x => x.Id);
            
            entity.HasMany<Product>(x => x.Products)
                .WithOne(x => x.Category)
                .HasPrincipalKey(x => x.Id)
                .HasForeignKey(x => x.CategoryId);
        });

        // Http
        builder.Entity<Proxy>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id)
                .ValueGeneratedOnAdd();
            
            entity.Property(x => x.Host)
                .IsRequired();
            entity.HasIndex(x => x.Host)
                .IsUnique();
            
            entity.Property(x => x.Port)
                .IsRequired();
        });

        builder.Entity<Cookie>(entity =>
        {
            entity.Property(x => x.YoulaAuth)
                .IsRequired();
            entity.HasIndex(x => x.YoulaAuth)
                .IsUnique();

            entity.Property(x => x.YoulaAuthRefresh)
                .IsRequired();
            entity.HasIndex(x => x.YoulaAuthRefresh)
                .IsUnique();
        });
        
        base.OnModelCreating(builder);
    }
}