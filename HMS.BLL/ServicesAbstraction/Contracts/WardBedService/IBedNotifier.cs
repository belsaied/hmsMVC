using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.ServicesAbstraction.Contracts.WardBedService
{
    public interface IBedNotifier
    {
        Task NotifyWardAsync(int wardId, string eventName, object payload);
        Task NotifyDashboardAsync(string eventName, object payload);

    }
}
