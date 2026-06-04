using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Services.Implementations.NotificationModule
{
    public static class TemplateRenderer
    {
        public static string Render(string template, Dictionary<string, string?> values)
        {
            if (string.IsNullOrEmpty(template))
                return string.Empty;

            foreach (var (key, value) in values)
            {
                template = template.Replace($"{{{{{key}}}}}", value ?? string.Empty,
                    StringComparison.OrdinalIgnoreCase);
            }

            return template;
        }
    }
}
