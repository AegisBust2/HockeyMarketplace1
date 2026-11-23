using System.Windows;
using System.Windows.Threading;
using HockeyMarketplace.Data;

namespace HockeyMarketplace;

public partial class App : Application
{
    public static HockeyDbContext Db { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Любая необработанная ошибка в интерфейсе показывается окном,
        // а приложение продолжает работать (раньше оно просто закрывалось).
        DispatcherUnhandledException += OnUnhandledException;

        try
        {
            Db = new HockeyDbContext();
            Db.Database.EnsureCreated();   // создаёт файл базы и таблицы при первом запуске
            Db.Seed();                     // демо-данные и администратор

            var login = new LoginWindow();
            MainWindow = login;
            login.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Не удалось запустить приложение.\n\n" +
                "Подробности: " + ex.GetBaseException().Message + "\n\n" +
                "Файл базы данных: " + HockeyDbContext.DbPath + "\n" +
                "Если проблема повторяется — закройте приложение и удалите этот файл, " +
                "при следующем запуске он создастся заново.",
                "Ошибка запуска",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown();
        }
    }

    private static void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // Если в контексте остались «зависшие» несохранённые объекты после ошибки — сбрасываем их,
        // иначе следующая операция снова упадёт с той же ошибкой.
        try { Db?.ChangeTracker.Clear(); } catch { }

        MessageBox.Show(
            "Произошла непредвиденная ошибка, но приложение продолжит работу.\n\n" +
            e.Exception.GetBaseException().Message,
            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        e.Handled = true;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Db?.Dispose();
        base.OnExit(e);
    }
}
