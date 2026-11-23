using System.Windows;
using System.Windows.Controls;
using HockeyMarketplace.Models;
using HockeyMarketplace.Services;
using Microsoft.EntityFrameworkCore;

namespace HockeyMarketplace.Pages;

public partial class AdminPage : Page
{
    public AdminPage()
    {
        InitializeComponent();
        RefreshAll();
    }

    private void RefreshAll()
    {
        try
        {
            ProductsGrid.ItemsSource = App.Db.Products.Include(x => x.Category).Include(x => x.Brand).AsNoTracking().OrderBy(x => x.Id).ToList();
            UsersGrid.ItemsSource = App.Db.Users.AsNoTracking().OrderBy(x => x.Id).ToList();
            LogsGrid.ItemsSource = App.Db.AuditLogs.AsNoTracking().OrderByDescending(x => x.Id).Take(300).ToList();
        }
        catch (Exception ex)
        {
            ShowError("Не удалось загрузить данные", ex);
        }
    }

    private static void ShowError(string title, Exception ex)
    {
        App.Db.ChangeTracker.Clear();
        MessageBox.Show(ex.GetBaseException().Message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void Refresh_Click(object sender, RoutedEventArgs e) => RefreshAll();

    private void DeleteProduct_Click(object sender, RoutedEventArgs e)
    {
        if (ProductsGrid.SelectedItem is not Product product)
        {
            MessageBox.Show("Выберите товар в таблице.", "Удаление");
            return;
        }

        try
        {
            if (App.Db.OrderItems.Any(x => x.ProductId == product.Id))
            {
                MessageBox.Show("Нельзя удалить товар, участвующий в заказах.", "Удаление");
                return;
            }

            if (MessageBox.Show($"Удалить «{product.Name}»?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            var entity = App.Db.Products.First(x => x.Id == product.Id);
            App.Db.Products.Remove(entity);
            App.Db.SaveChanges();
            AuditService.Write("Удаление", $"Товар #{product.Id}");
            RefreshAll();
        }
        catch (Exception ex)
        {
            ShowError("Не удалось удалить товар", ex);
        }
    }

    private void AddProduct_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ProductDialog { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() != true) return;

        try
        {
            if (App.Db.Products.Any(x => x.Article == dialog.Product.Article))
            {
                MessageBox.Show("Товар с таким артикулом уже существует.", "Товар");
                return;
            }

            App.Db.Products.Add(dialog.Product);
            App.Db.SaveChanges();
            AuditService.Write("Добавление", $"Товар #{dialog.Product.Id}");
            RefreshAll();
        }
        catch (Exception ex)
        {
            ShowError("Не удалось добавить товар", ex);
        }
    }

    private void EditProduct_Click(object sender, RoutedEventArgs e)
    {
        if (ProductsGrid.SelectedItem is not Product selected)
        {
            MessageBox.Show("Выберите товар в таблице.", "Редактирование");
            return;
        }

        try
        {
            var product = App.Db.Products.First(x => x.Id == selected.Id);
            var dialog = new ProductDialog(product) { Owner = Window.GetWindow(this) };
            if (dialog.ShowDialog() != true) return;

            if (App.Db.Products.Any(x => x.Article == product.Article && x.Id != product.Id))
            {
                App.Db.ChangeTracker.Clear();
                MessageBox.Show("Товар с таким артикулом уже существует.", "Товар");
                return;
            }

            App.Db.SaveChanges();
            AuditService.Write("Изменение", $"Товар #{product.Id}");
            RefreshAll();
        }
        catch (Exception ex)
        {
            ShowError("Не удалось сохранить товар", ex);
        }
    }

    private void ToggleBlock_Click(object sender, RoutedEventArgs e)
    {
        if (UsersGrid.SelectedItem is not User selected)
        {
            MessageBox.Show("Выберите пользователя в таблице.", "Блокировка");
            return;
        }

        if (selected.Id == AppSession.CurrentUser?.Id)
        {
            MessageBox.Show("Нельзя заблокировать самого себя.", "Блокировка");
            return;
        }

        try
        {
            var user = App.Db.Users.First(x => x.Id == selected.Id);
            user.IsBlocked = !user.IsBlocked;
            App.Db.SaveChanges();
            AuditService.Write(user.IsBlocked ? "Блокировка" : "Разблокировка", $"Пользователь #{user.Id}");
            RefreshAll();
        }
        catch (Exception ex)
        {
            ShowError("Не удалось изменить блокировку", ex);
        }
    }
}
