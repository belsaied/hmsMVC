using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.PL.Hubs
{
    public class WardHub : Hub
    {
        // Join ward-specific group to receive BedOccupied / BedReleased / BedTransferred / BedStatusChanged
        public async Task JoinWard(int wardId)
            => await Groups.AddToGroupAsync(Context.ConnectionId, $"ward-{wardId}");

        public async Task LeaveWard(int wardId)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ward-{wardId}");

        // FIX: BRD specifies group name "bed-dashboard" not "dashboard"
        public async Task JoinDashboard()
            => await Groups.AddToGroupAsync(Context.ConnectionId, "bed-dashboard");

        public async Task LeaveDashboard()
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, "bed-dashboard");
    }
}
