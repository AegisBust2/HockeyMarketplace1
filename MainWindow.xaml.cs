using System.Windows;
using HockeyMarketplace.Pages;
using HockeyMarketplace.Services;

namespace HockeyMarketplace;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        if (AppSession.CurrentUser?.Role == "Администратор")
            AdminButton.Visibility = Visibility.Visible;
        MainFrame.Navigate(new HomePage());
    }

    private void Home_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new HomePage());
    private void Cart_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new CartPage());
    private void Orders_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new OrdersPage());
    private void Profile_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ProfilePage());
    private void Admin_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AdminPage());

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Application.Current.Shutdown();
    }
}
