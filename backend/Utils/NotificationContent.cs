using System.Globalization;
using System.Resources;

namespace backend.Utils;

public static class NotificationContent
{
    private static readonly ResourceManager ResourceManager =
        new ResourceManager("backend.Utils.NotificationTemplates", typeof(NotificationContent).Assembly);

    public static string GetNotificationTemplate(string key, params object[] args)
    {
        var template = ResourceManager.GetString(key, CultureInfo.InvariantCulture);
        return args.Length > 0 ? string.Format(template, args) : template;
    }
}