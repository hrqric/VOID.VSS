using Dapper;
using VOID.VSS.Application.Commands.Components.Stock.Stock.Queries;
using VOID.VSS.Domain.Models.Components;
using VOID.VSS.Infrastructure.Configurations.Dapper.Enum;
using VOID.VSS.Infrastructure.Configurations.Dapper.Interfaces;

namespace VOID.VSS.Application.Queries;

public class ComponentQueryHandler(IDapperWrapper dapperWrapper)
{
    public async Task<dynamic> GetComponentHandleAsync(GetComponentQuery query, CancellationToken cancellationToken)
    {
        DynamicParameters parameters = new();
        List<string> queryWhere = new();

    #region Where

        if (query.ComponentId != null)
        {
            parameters.Add("componentId", query.ComponentId);
            queryWhere.Add("s.\"componentId\" = @componentId");
        }

        if (query.ComponentName != null)
        {
            parameters.Add("componentName", query.ComponentName);
            queryWhere.Add("s.\"componentName\" = @componentName");
        }

        if (query.Address != null)
        {
            parameters.Add("address", query.Address);
            queryWhere.Add("s.\"address\" = @address");
        }
        
        string where = string.Join(" AND ", queryWhere);
        
        #endregion
        
        var databaseQuery = queryWhere.Count > 0 ? $"""
                        SELECT
                            "componentId" AS "Id",
                            "componentName" AS "ComponentName",
                            "componentClass" AS "ComponentClass",
                            "address" AS "Address"
                        FROM stock s
                        WHERE {where}
                      """ : $"""
                        SELECT
                            "componentId" AS "Id",
                            "componentName" AS "ComponentName",
                            "componentClass" AS "ComponentClass",
                            "address" AS "Address"
                        FROM stock
                      """;
        var result =
            await dapperWrapper.GetRecordsAsync<ComponentViewModel>(EDatabase.Postgres, databaseQuery,
                cancellationToken, parameters);
        
        return result;
    }
}