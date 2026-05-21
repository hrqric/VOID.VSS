using Dapper;
using VOID.VSS.Application.Commands.Components.Stock.Movements;
using VOID.VSS.Domain.Enums;
using VOID.VSS.Domain.Models.Components;
using VOID.VSS.Infrastructure.Configurations.Dapper.Enum;
using VOID.VSS.Infrastructure.Configurations.Dapper.Interfaces;

namespace VOID.VSS.Application.Commands.Components.Stock;

public class ComponentCommandHandler(IDapperWrapper dapperWrapper)
{
    public async Task<dynamic> InsertComponentHandleAsync(InsertComponentCommand command, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        MovementCommandHandler movementCommandHandler = new(dapperWrapper);
        DynamicParameters parameters = new();
        var componentId = Guid.NewGuid();
        
        var query = $"""
                        INSERT INTO stock ("componentId", "componentName", "componentClass", "createdDate", "address", "details", "price", "status")
                        VALUES (@Id, @ComponentName, @ComponentClass, @CreatedDate, @Address, @Details, @Price, @Status);
                     """;
        
        parameters.AddDynamicParams(new
        { 
            Id = componentId,
            ComponentName = command.ComponentName,
            ComponentClass = command.ComponentClass.ToString(),
            CreatedDate = DateTime.Now,
            Address = command.Address,
            Details = command.Details,
            Price = command.Value,
            Status = command.Status.ToString()
        });
        
        await dapperWrapper.ExecuteQuery(EDatabase.Postgres, query, cancellationToken, parameters);

        ComponentViewModel result = new()
        {
            Id = componentId,
            ComponentName = command.ComponentName,
            ComponentClass = command.ComponentClass.ToString(),
            Address = command.Address
        };
        
        await movementCommandHandler.InsertMovementAsync(new InsertMovementCommand()
        {
            ComponentId = componentId,
            MovementType = EMovementType.Recebido,
            Quantity = command.Quantity,
            UserId = Guid.NewGuid().ToString()
        }, cancellationToken);
        
        
        return result;
    }

}