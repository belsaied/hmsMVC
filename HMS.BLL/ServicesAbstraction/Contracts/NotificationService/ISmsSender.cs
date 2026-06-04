using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.ServicesAbstraction.Contracts.NotificationService
{
    public interface ISmsSender
    {
        Task<string?> SendAsync(string toPhone, string body);
    }
}
