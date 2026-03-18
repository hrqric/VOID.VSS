using Dapper;
using VOID.VSS.Domain.Models.Components;
using VOID.VSS.Infrastructure.Configurations.Dapper.Enum;
using VOID.VSS.Infrastructure.Configurations.Dapper.Interfaces;

namespace VOID.VSS.Application.Commands.Address;

public class AddressCommandHandler(IDapperWrapper dapperWrapper)
{
    public async Task<dynamic> InsertAddressAsync(InsertAddressCommand command, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        DynamicParameters parameters = new();
        var query = $"""
                        INSERT INTO address (address, addressclass) VALUES (@address, @addressClass)
                     """;
        
        parameters.AddDynamicParams(new
        {
            address = command.AddressId,
            addressClass = command.Classification.ToString()
        });
        
        await dapperWrapper.ExecuteQuery(EDatabase.Postgres, query, cancellationToken, parameters);
        AddressViewModel result = new()
        {
            AddressId = command.AddressId,
            Classification = command.Classification.ToString()
        };
        return result;
    }
}