using VOID.VSS.Application.DTOs;
using VOID.VSS.Domain.Enums;
using VOID.VSS.Infrastructure.Configurations.Dapper.Enum;
using VOID.VSS.Infrastructure.Configurations.Dapper.Interfaces;

namespace VOID.VSS.Application.Commands.Components.Stock;

public class StockQuantityEngine(IDapperWrapper dW)
{
    public async Task<dynamic> UpdateStockQuantity(MovementDto mDto)
    {
        var exists = IsExistentComponent(mDto.ComponentId).Result;
        if (!exists)
        {
            string query = $"""
                            INSERT INTO stock_quantity ("componentid", "id", "itemquantity", "lastmov")
                            VALUES (@componentid, @id, @itemquantity, @lastmovementid)
                            """;
            await dW.ExecuteQuery(EDatabase.Postgres, query, CancellationToken.None, new
            {
                componentid = mDto.ComponentId,
                id = Guid.NewGuid(),
                itemquantity = mDto.MovementType == EMovementType.Recebido ? mDto.Quantity : -mDto.Quantity,
                lastmovementid = mDto.MovId
            });
            return 200;
        }
        int currentQuantity = await GetCurrentQuantityFromDatabase(mDto.ComponentId);
        int newQuantity = CalculateNewQuantity(currentQuantity, mDto.Quantity, mDto.MovementType);
        await UpdateQuantityInDatabase(mDto.ComponentId,mDto.MovId, newQuantity);

        return 201;
    }

    private async Task<bool> IsExistentComponent(Guid componentId)
    {
        string query = $"""
                        SELECT 1 FROM stock_quantity 
                        WHERE "componentid" = @componentId
                        """;
        return dW.GetRecordAsync<bool>(EDatabase.Postgres, query, CancellationToken.None, new { componentId }).Result;
    }

    private async Task<int> GetCurrentQuantityFromDatabase(Guid componentId)
    {
        string query = $"""
                        SELECT "itemquantity" FROM stock_quantity 
                                              WHERE "componentid" = @componentId
                      """;
        var current = await dW.GetRecordAsync<int>(EDatabase.Postgres, query, CancellationToken.None, new { componentId });
        
        return current;
    }
    
    private int CalculateNewQuantity(int currentQuantity, int movementQuantity, EMovementType movementType)
    {
        return movementType switch
        {
            EMovementType.Recebido => currentQuantity + movementQuantity,
            EMovementType.Retirada => currentQuantity - movementQuantity,
            _ => throw new ArgumentException("Invalid movement type")
        };
    }

    private async Task<int> UpdateQuantityInDatabase(Guid componentId, Guid movId, int newQuantity)
    {
        string query = $"""
                        UPDATE stock_quantity 
                        SET "itemquantity" = @newQuantity, "lastmov" = @movId
                        WHERE "componentid" = @componentId
                      """;
        await dW.ExecuteQuery(EDatabase.Postgres, query, CancellationToken.None, new { componentId, newQuantity, movId });
        return 201;
    }
}