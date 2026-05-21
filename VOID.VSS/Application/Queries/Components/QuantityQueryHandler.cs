using Dapper;
using VOID.VSS.Application.Commands.Components.Stock.Stock.Queries;
using VOID.VSS.Infrastructure.Configurations.Dapper.Enum;
using VOID.VSS.Infrastructure.Configurations.Dapper.Interfaces;

namespace VOID.VSS.Application.Queries;

public class QuantityQueryHandler(IDapperWrapper dapperWrapper)
{
    public async Task<int> GetQuantityHandleAsync(GetQuantityQuery query, CancellationToken cancellationToken)
    {
        DynamicParameters parameters = new();
        parameters.Add("componentId", query.ComponentId);
        
        var databaseQuery = $"""
                        SELECT
                            "itemquantity"
                        FROM stock_quantity
                        WHERE "componentid" = @componentId
                      """;
        var result =
            await dapperWrapper.GetRecordAsync<int>(EDatabase.Postgres, databaseQuery,
                cancellationToken, parameters);
        
        return result;
    }
}