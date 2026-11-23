using System.Windows;
using HockeyMarketplace.Models;

namespace HockeyMarketplace.Pages;

public partial class ProductDialog : Window
{
    public Product Product { get; private set; }

    public ProductDialog(Product? product = null)
    {
        InitializeComponent();
        Product = product ?? new Product { CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };

        foreach (var c in App.Db.Categories.OrderBy(x => x.Name)) CategoryBox.Items.Add(c);
        foreach (var b in App.Db.Brands.OrderBy(x => x.Name)) BrandBox.Items.Add(b);

        if (CategoryBox.Items.Count > 0) CategoryBox.SelectedIndex = 0;
        if (BrandBox.Items.Count > 0) BrandBox.SelectedIndex = 0;

        if (product != null)
        {
            NameBox.Text = product.Name; ArticleBox.Text = product.Article;
            CategoryBox.SelectedValue = product.CategoryId; BrandBox.SelectedValue = product.BrandId;
            SizeBox.Text = product.Size; ColorBox.Text = product.Color;
            PurchaseBox.Text = product.PurchasePrice.ToString(); RetailBox.Text = product.RetailPrice.ToString();
            QuantityBox.Text = product.Quantity.ToString(); MinBox.Text = product.MinQuantity.ToString();
            CountryBox.Text = product.Country; ImageBox.Text = product.ImagePath; DescriptionBox.Text = product.Description;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text) || string.IsNullOrWhiteSpace(ArticleBox.Text))
        {
            MessageBox.Show("Укажите наименование и артикул.", "Товар");
            return;
        }

        if (CategoryBox.SelectedItem is not Category category || BrandBox.SelectedItem is not Brand brand)
        {
            MessageBox.Show("Выберите категорию и бренд.", "Товар");
            return;
        }

        if (!decimal.TryParse(PurchaseBox.Text, out var purchase) ||
            !decimal.TryParse(RetailBox.Text, out var retail) ||
            !int.TryParse(QuantityBox.Text, out var quantity) ||
            !int.TryParse(MinBox.Text, out var min) ||
            purchase < 0 || retail < 0 || quantity < 0 || min < 0)
        {
            MessageBox.Show("Проверьте числовые поля: значения должны быть числами не меньше нуля.", "Товар");
            return;
        }

        Product.Name = NameBox.Text.Trim();
        Product.Article = ArticleBox.Text.Trim();
        Product.CategoryId = category.Id;
        Product.BrandId = brand.Id;
        Product.Size = SizeBox.Text.Trim();
        Product.Color = ColorBox.Text.Trim();
        Product.PurchasePrice = purchase;
        Product.RetailPrice = retail;
        Product.Quantity = quantity;
        Product.MinQuantity = min;
        Product.Country = CountryBox.Text.Trim();
        Product.ImagePath = ImageBox.Text.Trim();
        Product.Description = DescriptionBox.Text.Trim();
        Product.UpdatedAt = DateTime.Now;

        DialogResult = true;
        Close();
    }
}
