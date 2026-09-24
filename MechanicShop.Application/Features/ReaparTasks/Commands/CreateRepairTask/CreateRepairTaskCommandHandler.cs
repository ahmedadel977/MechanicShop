using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MechanicShop.Application.Features.ReaparTasks.Commands.CreateRepairTask
{
    public class CreateRepairTaskCommandHandler(IAppDbContext context, HybridCache cache, ILogger<CreateRepairTaskCommandHandler> logger) : IRequestHandler<CreateRepairTaskCommand, Result<RepairTaskDto>>
    {
        private readonly IAppDbContext _context = context;
        private readonly HybridCache _cache = cache;
        private readonly ILogger<CreateRepairTaskCommandHandler> _logger = logger;
        public async Task<Result<RepairTaskDto>> Handle(CreateRepairTaskCommand command, CancellationToken cancellationToken)
        {
            var nameExists = await _context.RepairTasks.AnyAsync(rt => rt.Name == command.Name, cancellationToken);
            if (nameExists)
            {
                _logger.LogWarning("Duplicate part name '{PartName}'.", command.Name);

                return RepairTaskErrors.DuplicateName;
            }
            List<Part> parts = [];
            foreach (var partCommand in command.Parts)
            {
                var partResult = Part.Create(Guid.NewGuid(), partCommand.Name, partCommand.Cost, partCommand.Quantity);
                if (partResult.IsError)
                {
                    _logger.LogWarning("Failed to create part '{PartName}", partCommand.Name);
                    return partResult.Errors;
                }
                parts.Add(partResult.Value);
            }
            var repairTaskResultcreate = RepairTask.Create(Guid.NewGuid(), command.Name, command.LaborCost, command.EstimatedDurationInMin!.Value, parts);
            if (repairTaskResultcreate.IsError)
            {
                _logger.LogWarning("Failed to create repair task '{RepairTaskName}", command.Name);
                return repairTaskResultcreate.Errors;
            }
            var repairTask = repairTaskResultcreate.Value;
            _context.RepairTasks.Add(repairTask);
            await _context.SaveChangesAsync(cancellationToken);
            await _cache.RemoveAsync("repair-task", cancellationToken);
            return repairTask.ToDto();



        }
    }
}


