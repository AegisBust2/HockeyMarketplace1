using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HockeyMarketplace.Models;
using Microsoft.EntityFrameworkCore;

namespace HockeyMarketplace.Pages;

public partial class HomePage : Page
{
    private List<Product> _products = new();

    public HomePage()
    {
        InitializeComponent();
        LoadFilters();
        // Товары грузим после того, как страница полностью загрузилась
        // (в конструкторе IsLoaded ещё false, и список оставался пустым).
        Loaded += (_, _) => LoadProducts();
    }

    private void LoadFilters()
    {
        CategoryBox.Items.Add("Все категории");
        foreach (var c in App.Db.Categories.OrderBy(x => x.Name)) CategoryBox.Items.Add(c.Name);
        CategoryBox.SelectedIndex = 0;

        BrandBox.Items.Add("Все бренды");
        foreach (var b in App.Db.Brands.OrderBy(x => x.Name)) BrandBox.Items.Add(b.Name);
        BrandBox.SelectedIndex = 0;
    }

    private void FilterChanged(object sender, SelectionChangedEventArgs e) => LoadProducts();
    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => LoadProducts();

    private void LoadProducts()
    {
        if (!IsLoaded) return;

        var search = SearchBox.Text?.Trim() ?? "";
        var category = CategoryBox.SelectedIndex > 0 ? CategoryBox.SelectedItem?.ToString() : null;
        var brand = BrandBox.SelectedIndex > 0 ? BrandBox.SelectedItem?.ToString() : null;

        try
        {
            // SQLite умеет приводить к нижнему регистру только латиницу,
            // поэтому поиск и фильтры выполняем в памяти (товаров немного).
            var all = App.Db.Products
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .AsNoTracking()
                .ToList();

            IEnumerable<Product> filtered = all;

            if (!string.IsNullOrEmpty(search))
                filtered = filtered.Where(x =>
                    x.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                    x.Article.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                    (x.Brand?.Name ?? "").Contains(search, StringComparison.CurrentCultureIgnoreCase));

            if (category != null)
                filtered = filtered.Where(x => x.Category?.Name == category);

            if (brand != null)
                filtered = filtered.Where(x => x.Brand?.Name == brand);

            _products = filtered.OrderBy(x => x.Name).ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не удалось загрузить товары: " + ex.GetBaseException().Message,
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        ProductsList.Items.Clear();

        foreach (var product in _products)
            ProductsList.Items.Add(CreateCard(product));
    }

    private Border CreateCard(Product product)
    {
        var card = new Border
        {
            Width = 285, Margin = new Thickness(8), Padding = new Thickness(12),
            Background = Brushes.White, CornerRadius = new CornerRadius(12),
            BorderBrush = new SolidColorBrush(Color.FromRgb(226,232,240)), BorderThickness = new Thickness(1)
        };

        var stack = new StackPanel();
        var img = new Image { Height = 170, Stretch = System.Windows.Media.Stretch.Uniform };
        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, product.ImagePath), UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;   // файл не блокируется после загрузки
            bmp.EndInit();
            img.Source = bmp;
        }
        catch { }

        stack.Children.Add(img);
        stack.Children.Add(new TextBlock { Text = product.Name, FontWeight = FontWeights.SemiBold, FontSize = 16, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,10,0,4) });
        stack.Children.Add(new TextBlock { Text = $"{product.Brand?.Name} • {product.Category?.Name}", Foreground = new SolidColorBrush(Color.FromRgb(100,116,139)) });
        stack.Children.Add(new TextBlock { Text = product.Description, FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(100,116,139)), TextWrapping = TextWrapping.Wrap, Height = 38, Margin = new Thickness(0,7,0,0) });
        stack.Children.Add(new TextBlock { Text = $"В наличии: {product.Quantity} шт.", FontSize = 12, Margin = new Thickness(0,4,0,4) });

        var bottom = new Grid();
        bottom.ColumnDefinitions.Add(new ColumnDefinition());
        bottom.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        bottom.Children.Add(new TextBlock { Text = $"{product.RetailPrice:N0} ₽", FontSize = 21, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(18,62,120)), VerticalAlignment = VerticalAlignment.Center });
        var button = new Button { Content = "Добавить", Padding = new Thickness(15,7,15,7), Tag = product, IsEnabled = product.Quantity > 0 };
        button.Click += Add_Click;
        Grid.SetColumn(button, 1);
        bottom.Children.Add(button);
        stack.Children.Add(bottom);

        card.Child = stack;
        return card;
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        var product = (Product)((Button)sender).Tag;
        HockeyMarketplace.Services.AppSession.AddToCart(product);
        MessageBox.Show($"«{product.Name}» добавлен в корзину.", "Корзина", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
