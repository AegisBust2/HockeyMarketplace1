using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HockeyMarketplace.Models;
using HockeyMarketplace.Services;

namespace HockeyMarketplace.Pages;

public partial class CartPage : Page
{
    public CartPage()
    {
        InitializeComponent();
        Refresh();
    }

    private void Refresh()
    {
        CartList.Items.Clear();

        foreach (var item in AppSession.Cart.ToList())
        {
            var border = new Border
            {
                Background = Brushes.White, CornerRadius = new CornerRadius(10),
                Margin = new Thickness(0,0,12,10), Padding = new Thickness(14)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(38) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var check = new CheckBox { IsChecked = item.IsSelected, VerticalAlignment = VerticalAlignment.Center };
            check.Checked += (_,_) => { item.IsSelected = true; UpdateTotal(); };
            check.Unchecked += (_,_) => { item.IsSelected = false; UpdateTotal(); };
            grid.Children.Add(check);

            var info = new StackPanel();
            info.Children.Add(new TextBlock { Text = item.Product.Name, FontSize = 16, FontWeight = FontWeights.SemiBold });
            info.Children.Add(new TextBlock { Text = $"{item.Product.RetailPrice:N0} ₽ • {item.Product.Size} • {item.Product.Color}", Foreground = new SolidColorBrush(Color.FromRgb(100,116,139)) });
            Grid.SetColumn(info,1);
            grid.Children.Add(info);

            var controls = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            var minus = new Button { Content = "−", Padding = new Thickness(10,5,10,5) };
            minus.Click += (_,_) => ChangeQuantity(item, -1);
            var qty = new TextBlock { Text = item.Quantity.ToString(), Width=30, TextAlignment=TextAlignment.Center, VerticalAlignment=VerticalAlignment.Center };
            var plus = new Button { Content = "+", Padding = new Thickness(10,5,10,5) };
            plus.Click += (_,_) => ChangeQuantity(item, 1);
            var remove = new Button { Content = "Удалить", Background=Brushes.Transparent, Foreground=new SolidColorBrush(Color.FromRgb(185,28,28)) };
            remove.Click += (_,_) => { AppSession.Cart.Remove(item); Refresh(); };
            controls.Children.Add(minus); controls.Children.Add(qty); controls.Children.Add(plus); controls.Children.Add(remove);
            Grid.SetColumn(controls,2);
            grid.Children.Add(controls);

            border.Child = grid;
            CartList.Items.Add(border);
        }

        UpdateTotal();
    }

    private void ChangeQuantity(CartItem item, int delta)
    {
        item.Quantity = Math.Clamp(item.Quantity + delta, 1, item.Product.Quantity);
        Refresh();
    }

    private void UpdateTotal() => TotalText.Text = $"{AppSession.SelectedTotal(AppSession.Cart.Where(x => x.IsSelected)):N0} ₽";

    private void Checkout_Click(object sender, RoutedEventArgs e)
    {
        var selected = AppSession.Cart.Where(x => x.IsSelected).ToList();
        if (!selected.Any())
        {
            MessageBox.Show("Выберите хотя бы один товар.", "Заказ");
            return;
        }
        if (DeliveryBox.SelectedIndex == 0 && string.IsNullOrWhiteSpace(AddressBox.Text))
        {
            MessageBox.Show("Укажите адрес доставки.", "Заказ");
            return;
        }

        using var tx = App.Db.Database.BeginTransaction();
        try
        {
            var order = new Order
            {
                UserId = AppSession.CurrentUser!.Id,
                PaymentMethod = ((ComboBoxItem)PaymentBox.SelectedItem).Content.ToString()!,
                DeliveryMethod = ((ComboBoxItem)DeliveryBox.SelectedItem).Content.ToString()!,
                DeliveryAddress = AddressBox.Text.Trim(),
                Status = "Новый",
                Total = 0
            };
            App.Db.Orders.Add(order);
            App.Db.SaveChanges();

            decimal total = 0;
            foreach (var item in selected)
            {
                // Берём актуальные данные из базы (цена и остаток могли измениться)
                var product = App.Db.Products.First(x => x.Id == item.Product.Id);
                if (product.Quantity < item.Quantity)
                    throw new InvalidOperationException($"Недостаточно товара: {product.Name}");

                total += product.RetailPrice * item.Quantity;
                product.Quantity -= item.Quantity;
                App.Db.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id, ProductId = product.Id,
                    Quantity = item.Quantity, UnitPrice = product.RetailPrice
                });
                App.Db.StockMovements.Add(new StockMovement
                {
                    ProductId = product.Id, Type = "Продажа", Quantity = -item.Quantity,
                    Note = $"Заказ №{order.Id}"
                });
            }

            order.Total = total;

            App.Db.Payments.Add(new Payment
            {
                OrderId = order.Id, Amount = total,
                Method = order.PaymentMethod, Status = "Оплачен"
            });

            App.Db.SaveChanges();
            tx.Commit();

            foreach (var item in selected) AppSession.Cart.Remove(item);
            AuditService.Write("Заказ", $"Заказ #{order.Id}");
            MessageBox.Show($"Заказ №{order.Id} успешно оформлен.", "Заказ");
            Refresh();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            App.Db.ChangeTracker.Clear();   // убираем незавершённый заказ из контекста
            MessageBox.Show(ex.GetBaseException().Message, "Не удалось оформить заказ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
