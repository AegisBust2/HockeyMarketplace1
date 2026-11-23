using System.IO;
using HockeyMarketplace.Models;
using Microsoft.EntityFrameworkCore;

namespace HockeyMarketplace.Data;

public class HockeyDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    /// <summary>Файл базы данных: %LocalAppData%\HockeyMarketplace\hockey_market.db (создаётся автоматически).</summary>
    public static string DbPath
    {
        get
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HockeyMarketplace");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "hockey_market.db");
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Product>().HasIndex(x => x.Article).IsUnique();

        modelBuilder.Entity<Product>()
            .HasOne(x => x.Category).WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasOne(x => x.Brand).WithMany(x => x.Products)
            .HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(x => x.User).WithMany(x => x.Orders)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .HasOne(x => x.Order).WithMany(x => x.Items)
            .HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Seed()
    {
        if (!Categories.Any())
        {
            Categories.AddRange(
                new Category { Name = "Коньки" },
                new Category { Name = "Клюшки" },
                new Category { Name = "Шлемы" },
                new Category { Name = "Перчатки" },
                new Category { Name = "Защита" },
                new Category { Name = "Экипировка вратаря" },
                new Category { Name = "Одежда" },
                new Category { Name = "Сумки" },
                new Category { Name = "Аксессуары" });
            SaveChanges();
        }

        if (!Brands.Any())
        {
            Brands.AddRange(
                new Brand { Name = "Bauer" },
                new Brand { Name = "CCM" },
                new Brand { Name = "Warrior" },
                new Brand { Name = "True" },
                new Brand { Name = "Sherwood" });
            SaveChanges();
        }

        if (!Users.Any())
        {
            Users.Add(new User
            {
                FullName = "Администратор",
                Phone = "+7 900 000-00-00",
                Email = "admin@hockey.ru",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Администратор"
            });
            SaveChanges();
        }

        if (!Products.Any())
        {
            var c = Categories.ToDictionary(x => x.Name);
            var b = Brands.ToDictionary(x => x.Name);

            Products.AddRange(
                new Product { Name="Коньки Bauer Vapor X4", Article="SK-001", CategoryId=c["Коньки"].Id, BrandId=b["Bauer"].Id, Size="42", Color="Чёрный", PurchasePrice=19000, RetailPrice=24990, Quantity=12, MinQuantity=3, Country="Канада", ImagePath="Assets/skates.png", Description="Лёгкие хоккейные коньки для тренировок и матчей." },
                new Product { Name="Клюшка CCM Jetspeed FT6", Article="ST-002", CategoryId=c["Клюшки"].Id, BrandId=b["CCM"].Id, Size="SR", Color="Чёрный/белый", PurchasePrice=12000, RetailPrice=16990, Quantity=8, MinQuantity=2, Country="Канада", ImagePath="Assets/stick.png", Description="Композитная клюшка с быстрым выбросом шайбы." },
                new Product { Name="Шлем Bauer Re-Akt 155", Article="HE-003", CategoryId=c["Шлемы"].Id, BrandId=b["Bauer"].Id, Size="L", Color="Чёрный", PurchasePrice=9000, RetailPrice=12990, Quantity=15, MinQuantity=4, Country="Канада", ImagePath="Assets/helmet.png", Description="Защитный шлем с регулируемой посадкой." },
                new Product { Name="Перчатки Warrior Alpha LX2", Article="GL-004", CategoryId=c["Перчатки"].Id, BrandId=b["Warrior"].Id, Size="14", Color="Красный", PurchasePrice=7000, RetailPrice=9990, Quantity=10, MinQuantity=3, Country="США", ImagePath="Assets/gloves.png", Description="Манёвренные перчатки с усиленной защитой кисти." },
                new Product { Name="Нагрудник CCM Tacks XF", Article="SH-005", CategoryId=c["Защита"].Id, BrandId=b["CCM"].Id, Size="L", Color="Чёрный", PurchasePrice=11000, RetailPrice=15490, Quantity=6, MinQuantity=2, Country="Канада", ImagePath="Assets/shoulders.png", Description="Надёжная защита корпуса и плечевого пояса." },
                new Product { Name="Щитки Bauer Supreme M3", Article="PN-006", CategoryId=c["Защита"].Id, BrandId=b["Bauer"].Id, Size="17", Color="Белый/чёрный", PurchasePrice=6500, RetailPrice=8990, Quantity=14, MinQuantity=3, Country="Канада", ImagePath="Assets/pads.png", Description="Лёгкие щитки с хорошей защитой голени." },
                new Product { Name="Сумка CCM 390 Wheel", Article="BG-007", CategoryId=c["Сумки"].Id, BrandId=b["CCM"].Id, Size="XL", Color="Чёрный", PurchasePrice=5000, RetailPrice=7490, Quantity=9, MinQuantity=2, Country="Канада", ImagePath="Assets/bag.png", Description="Вместительная хоккейная сумка на колёсах." },
                new Product { Name="Термобельё Warrior", Article="CL-008", CategoryId=c["Одежда"].Id, BrandId=b["Warrior"].Id, Size="L", Color="Чёрный", PurchasePrice=3000, RetailPrice=4990, Quantity=20, MinQuantity=5, Country="США", ImagePath="Assets/thermal.png", Description="Термобельё для тренировок и активной игры." },
                new Product { Name="Шайба тренировочная CCM", Article="AC-009", CategoryId=c["Аксессуары"].Id, BrandId=b["CCM"].Id, Size="STD", Color="Чёрный", PurchasePrice=250, RetailPrice=490, Quantity=100, MinQuantity=20, Country="Канада", ImagePath="Assets/puck.png", Description="Стандартная шайба для тренировок и матчей." }
            );
            SaveChanges();
        }
    }
}
