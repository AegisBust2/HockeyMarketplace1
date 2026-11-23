using Microsoft.EntityFrameworkCore;

namespace HockeyMarketplace.Services;

public static class AuditService
{
    public static void Write(string action, string objectName)
    {
        Models.AuditLog? log = null;
        try
        {
            var user = AppSession.CurrentUser;
            log = new Models.AuditLog
            {
                UserId = user?.Id,
                UserName = user?.FullName ?? "Система",
                Action = action,
                ObjectName = objectName
            };
            App.Db.AuditLogs.Add(log);
            App.Db.SaveChanges();
        }
        catch
        {
            // Журнал не должен ломать работу приложения. Если запись не сохранилась,
            // отсоединяем её, чтобы она не мешала следующим сохранениям.
            if (log != null)
                App.Db.Entry(log).State = EntityState.Detached;
        }
    }
}
