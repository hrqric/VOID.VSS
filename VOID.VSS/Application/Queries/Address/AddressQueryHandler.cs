using Dapper;
using VOID.VSS.Domain.Models.Components;
using VOID.VSS.Infrastructure.Configurations.Dapper.Enum;
using VOID.VSS.Infrastructure.Configurations.Dapper.Interfaces;

namespace VOID.VSS.Application.Queries.Address;

public class AddressQueryHandler(IDapperWrapper dapperWrapper)
{
    public async Task<dynamic> GetAddressHandleAsync(GetAddressQuery query, CancellationToken cancellationToken)
    {
        DynamicParameters parameters = new();
        List<string> queryWhere = new();

        #region Where

        if (query.AddressId != null)
        {
            queryWhere.Add("address = @AddressId");
            parameters.Add("AddressId", query.AddressId);
        }

        if (query.Classification != null)
        {
            queryWhere.Add("addressclass = @Classification");
            parameters.Add("Classification", query.Classification.ToString());
        }

        #endregion

        var databaseQuery = queryWhere.Count > 0 ? 
        """
        SELECT address, addressclass FROM address WHERE 
        """ + string.Join(" AND ", queryWhere) :
        $"""  SELECT address as AddressId, addressclass as Classification FROM address """;
        
        var result = await dapperWrapper.GetRecordsAsync<AddressViewModel>(EDatabase.Postgres, 
            databaseQuery, cancellationToken, parameters);

        return result;
    }
}