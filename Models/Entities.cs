using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HockeyMarketplace.Models;

public class User
{
    [Key] public int Id { get; set; }
    [MaxLength(120)] public string FullName { get; set; } = "";
    [MaxLength(30)] public string Phone { get; set; } = "";
    [MaxLength(150)] public string Email { get; set; } = "";
    [MaxLength(100)] public string PasswordHash { get; set; } = "";
    [MaxLength(30)] public string Role { get; set; } = "Покупатель";
    public DateTime RegisteredAt { get; set; } = DateTime.Now;
    public bool IsBlocked { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

public class Category
{
    [Key] public int Id { get; set; }
    [MaxLength(80)] public string Name { get; set; } = "";
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Brand
{
    [Key] public int Id { get; set; }
    [MaxLength(80)] public string Name { get; set; } = "";
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Supplier
{
    [Key] public int Id { get; set; }
    [MaxLength(150)] public string OrganizationName { get; set; } = "";
    [MaxLength(120)] public string ContactPerson { get; set; } = "";
    [MaxLength(30)] public string Phone { get; set; } = "";
    [MaxLength(150)] public string Email { get; set; } = "";
}

public class Product
{
    [Key] public int Id { get; set; }
    [MaxLength(160)] public string Name { get; set; } = "";
    [MaxLength(50)] public string Article { get; set; } = "";
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int BrandId { get; set; }
    public Brand? Brand { get; set; }
    [MaxLength(30)] public string Size { get; set; } = "";
    [MaxLength(60)] public string Color { get; set; } = "";
    public decimal PurchasePrice { get; set; }
    public decimal RetailPrice { get; set; }
    public int Quantity { get; set; }
    public int MinQuantity { get; set; }
    [MaxLength(80)] public string Country { get; set; } = "";
    [MaxLength(300)] public string ImagePath { get; set; } = "";
    [MaxLength(1000)] public string Description { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

public class Order
{
    [Key] public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    [MaxLength(40)] public string Status { get; set; } = "Новый";
    [MaxLength(40)] public string PaymentMethod { get; set; } = "";
    [MaxLength(500)] public string DeliveryAddress { get; set; } = "";
    [MaxLength(40)] public string DeliveryMethod { get; set; } = "Курьер";
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    [Key] public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    [NotMapped] public decimal LineTotal => Quantity * UnitPrice;
}

public class Payment
{
    [Key] public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public decimal Amount { get; set; }
    [MaxLength(40)] public string Method { get; set; } = "";
    [MaxLength(30)] public string Status { get; set; } = "Оплачен";
    public DateTime PaidAt { get; set; } = DateTime.Now;
}

public class StockMovement
{
    [Key] public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    [MaxLength(40)] public string Type { get; set; } = "";
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    [MaxLength(250)] public string Note { get; set; } = "";
}

public class AuditLog
{
    [Key] public int Id { get; set; }
    public int? UserId { get; set; }
    [MaxLength(120)] public string UserName { get; set; } = "";
    [MaxLength(60)] public string Action { get; set; } = "";
    [MaxLength(120)] public string ObjectName { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
