using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Common.NotificationSettings
{
    public class TwilioSettings
    {
        public string AccountSid { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string FromNumber { get; set; } = string.Empty;
    }
}
