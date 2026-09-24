using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace MechanicShop.Infrastructure.Realtime
{
    public sealed class WorkOrderHub : Hub
    {
        public const string HubUrl = "/hubs/workorders";
    }
}
