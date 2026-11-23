using System.Windows;
using Microsoft.EntityFrameworkCore;
using HockeyMarketplace.Services;

namespace HockeyMarketplace;

public partial class LoginWindow : Window
{
    public LoginWindow() => InitializeComponent();

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        var email = EmailBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
        {
            MessageBox.Show("Введите email и пароль.", "Вход",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var emailLower = email.ToLower();
            var user = App.Db.Users.AsNoTracking()
                .FirstOrDefault(x => x.Email.ToLower() == emailLower);

            if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                MessageBox.Show("Неверный email или пароль.", "Вход",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (user.IsBlocked)
            {
                MessageBox.Show("Пользователь заблокирован.", "Вход",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AppSession.CurrentUser = user;
            AppSession.Cart.Clear();
            AuditService.Write("Вход", "Пользователь");

            var main = new MainWindow();
            Application.Current.MainWindow = main;
            main.Show();
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Не удалось выполнить вход.\n\n" +
                "Подробности: " + ex.GetBaseException().Message,
                "Ошибка авторизации",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void Register_Click(object sender, RoutedEventArgs e)
    {
        new RegisterWindow { Owner = this }.ShowDialog();
    }
}
