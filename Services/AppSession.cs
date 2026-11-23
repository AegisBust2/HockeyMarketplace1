using HockeyMarketplace.Models;

namespace HockeyMarketplace.Services;

public static class AppSession
{
    public static User? CurrentUser { get; set; }
    public static List<CartItem> Cart { get; } = new();

    public static void AddToCart(Product product)
    {
        var existing = Cart.FirstOrDefault(x => x.Product.Id == product.Id);
        if (existing is null)
            Cart.Add(new CartItem(product, 1));
        else if (existing.Quantity < product.Quantity)
            existing.Quantity++;
    }

    public static decimal SelectedTotal(IEnumerable<CartItem> items) =>
        items.Sum(x => x.Product.RetailPrice * x.Quantity);
}

public class CartItem
{
    public Product Product { get; }
    public int Quantity { get; set; }
    public bool IsSelected { get; set; } = true;

    public CartItem(Product product, int quantity)
    {
        Product = product;
        Quantity = quantity;
    }
}
