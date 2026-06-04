using Microsoft.AspNetCore.SignalR;
using HMS.BLL.ServicesAbstraction.Contracts.WardBedService;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.PL.Hubs
{
    public class BedNotifier(IHubContext<WardHub> _hubContext) : IBedNotifier
    {
        public async Task NotifyWardAsync(int wardId, string eventName, object payload)
        {
            await _hubContext.Clients
                .Group($"ward-{wardId}")
                .SendAsync(eventName, payload);
        }

        public async Task NotifyDashboardAsync(string eventName, object payload)
        {
            // FIX: BRD specifies group name "bed-dashboard" not "dashboard"
            await _hubContext.Clients
                .Group("bed-dashboard")
                .SendAsync(eventName, payload);
        }
    }
}
