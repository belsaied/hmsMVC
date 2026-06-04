using AutoMapper;
using HMS.DAL.Contracts;
using HMS.DAL.Models.Enums.NotificationEnums;
using HMS.DAL.Models.NotificationModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using HMS.BLL.ServicesAbstraction.Contracts.NotificationService;
using HMS.BLL.Services.Specifications.NotificationModule;
using HMS.BLL.Services.Specifications.NotificationModule.NotificationSpecification;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.PL.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinUserGroup(string userId)
            => await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

        public async Task LeaveUserGroup(string userId)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
    }
}
