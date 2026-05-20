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
                        INSERT INTO stock ("componentId", "componentName", "componentClass", "createdDate", "address")
                        VALUES (@Id, @ComponentName, @ComponentClass, @CreatedDate, @Address);
                     """;
        
        parameters.AddDynamicParams(new
        { 
            Id = componentId,
            ComponentName = command.ComponentName,
            ComponentClass = command.ComponentClass,
            CreatedDate = DateTime.Now,
            Address = command.Address
        });
        
        await dapperWrapper.ExecuteQuery(EDatabase.Postgres, query, cancellationToken, parameters);

        ComponentViewModel result = new()
        {
            Id = componentId,
            ComponentName = command.ComponentName,
            ComponentClass = command.ComponentClass,
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