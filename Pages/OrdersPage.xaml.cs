using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace HockeyMarketplace.Pages;

public partial class OrdersPage : Page
{
    public OrdersPage()
    {
        InitializeComponent();
        try
        {
            var userId = Services.AppSession.CurrentUser!.Id;
            OrdersGrid.ItemsSource = App.Db.Orders.AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Id).ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не удалось загрузить заказы: " + ex.GetBaseException().Message,
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
