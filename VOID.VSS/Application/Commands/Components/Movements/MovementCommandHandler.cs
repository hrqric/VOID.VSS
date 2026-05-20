using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using VOID.VSS.Application.DTOs;
using VOID.VSS.Infrastructure.Configurations.Dapper.Enum;
using VOID.VSS.Infrastructure.Configurations.Dapper.Interfaces;

namespace VOID.VSS.Application.Commands.Components.Stock.Movements;

public class MovementCommandHandler(IDapperWrapper dapperWrapper)
{
    public async Task<int> InsertMovementAsync(InsertMovementCommand command, CancellationToken ct)
    {
        DynamicParameters parameters = new();
        StockQuantityEngine sqe = new(dapperWrapper);
        var query = $"""
                        INSERT INTO stock_mov ("componentid", "movementid", "movquantity", "movementtype", "movdate")
                        VALUES (@Id, @mId, @qtd, @type, @movDate);
                     """;
        var movementId = Guid.NewGuid();
        parameters.AddDynamicParams(new
        {
            Id = command.ComponentId,
            mId = movementId,
            qtd = command.Quantity,
            type = command.MovementType.ToString(),
            movDate = DateTime.Now,
            //userId = Guid.Parse(command.UserId)
        });

        await dapperWrapper.ExecuteQuery(EDatabase.Postgres, query, ct, parameters);
        
        var movement = new MovementDto()
        {
            ComponentId = command.ComponentId,
            Quantity = command.Quantity,
            MovementType = command.MovementType,
            MovId = movementId
        };
        return await sqe.UpdateStockQuantity(movement);
    }

}