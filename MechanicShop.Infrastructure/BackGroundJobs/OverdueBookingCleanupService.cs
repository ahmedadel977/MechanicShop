using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Workorders;
using MechanicShop.Domain.Workorders.Enums;
using MechanicShop.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MechanicShop.Infrastructure.BackGroundJobs
{
    public class OverdueBookingCleanupService(IServiceScopeFactory scopeFactory,
        ILogger<OverdueBookingCleanupService> logger,
        IOptions<AppSettings> options,
        TimeProvider timeProvider) : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly ILogger<OverdueBookingCleanupService> logger = logger;
        private readonly AppSettings _appSettings = options.Value;
        private readonly TimeProvider timeProvider = timeProvider;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_appSettings.OverdueBookingCleanupFrequencyMinutes));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                logger.LogInformation("Checking overdue work orders at {Now}", timeProvider.GetUtcNow());
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
                    var catOFH = timeProvider.GetUtcNow().AddMinutes(-_appSettings.BookingCancellationThresholdMinutes);
                    var overdue = await db.WorkOrders
                    .Where(w => w.State == WorkOrderState.Scheduled && w.StartAtUtc <= catOFH)
                    .ToListAsync(stoppingToken);
                    if(overdue.Count > 0)
                    {
                        logger.LogInformation("Found {Count} overdue work orders to cancel", overdue.Count);
                        foreach (var workOrder in overdue)
                        {
                           var  result=workOrder.Cancel();
                            if (result.IsError)
                            {
                                logger.LogError("Failed to cancel work order {WorkOrderId}: {Errors}", workOrder.Id, result.Errors);
                            }
                            await db.SaveChangesAsync(stoppingToken);

                            logger.LogInformation("Cancelling work order {WorkOrderId} scheduled for {StartAtUtc}", workOrder.Id, workOrder.StartAtUtc);

                        }
                        
                        

                    }
                    else
                    {
                        logger.LogInformation("No overdue work orders found");
                    }

                }
                catch
                (Exception ex)
                {
                    logger.LogError(ex, "Error occurred while checking overdue work orders");
                }



            }    
        }
    }
}
