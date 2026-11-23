using System.Windows;
using System.Windows.Controls;
using HockeyMarketplace.Services;

namespace HockeyMarketplace.Pages;

public partial class ProfilePage : Page
{
    public ProfilePage()
    {
        InitializeComponent();
        var u = AppSession.CurrentUser!;
        NameBox.Text = u.FullName;
        PhoneBox.Text = u.Phone;
        EmailBox.Text = u.Email;
        RoleBox.Text = u.Role;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        var email = EmailBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
        {
            MessageBox.Show("ФИО и email не могут быть пустыми.", "Личный кабинет");
            return;
        }

        try
        {
            var id = AppSession.CurrentUser!.Id;
            var emailLower = email.ToLower();
            if (App.Db.Users.Any(x => x.Id != id && x.Email.ToLower() == emailLower))
            {
                MessageBox.Show("Этот email уже используется другим пользователем.", "Личный кабинет");
                return;
            }

            var u = App.Db.Users.First(x => x.Id == id);
            u.FullName = name;
            u.Phone = PhoneBox.Text.Trim();
            u.Email = email;
            App.Db.SaveChanges();
            AppSession.CurrentUser = u;
            MessageBox.Show("Данные сохранены.", "Личный кабинет");
        }
        catch (Exception ex)
        {
            App.Db.ChangeTracker.Clear();
            MessageBox.Show("Не удалось сохранить: " + ex.GetBaseException().Message,
                "Личный кабинет", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
