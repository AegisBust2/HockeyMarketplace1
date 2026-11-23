using System.Windows;
using HockeyMarketplace.Models;

namespace HockeyMarketplace;

public partial class RegisterWindow : Window
{
    public RegisterWindow() => InitializeComponent();

    private void Register_Click(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        var email = EmailBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            PasswordBox.Password.Length < 6)
        {
            MessageBox.Show("Укажите ФИО, email и пароль не менее 6 символов.", "Регистрация");
            return;
        }

        if (!email.Contains('@') || !email.Contains('.'))
        {
            MessageBox.Show("Введите корректный email.", "Регистрация");
            return;
        }

        try
        {
            var emailLower = email.ToLower();
            if (App.Db.Users.Any(x => x.Email.ToLower() == emailLower))
            {
                MessageBox.Show("Пользователь с таким email уже существует.", "Регистрация");
                return;
            }

            App.Db.Users.Add(new User
            {
                FullName = name,
                Phone = PhoneBox.Text.Trim(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordBox.Password),
                Role = "Покупатель"
            });
            App.Db.SaveChanges();
            MessageBox.Show("Аккаунт создан. Теперь можно войти.", "Регистрация");
            Close();
        }
        catch (Exception ex)
        {
            App.Db.ChangeTracker.Clear();
            MessageBox.Show("Не удалось создать аккаунт: " + ex.GetBaseException().Message,
                "Регистрация", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
