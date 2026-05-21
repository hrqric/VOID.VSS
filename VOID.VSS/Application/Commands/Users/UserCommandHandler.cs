using Dapper;
using VOID.VSS.Infrastructure.Configurations.Dapper.Enum;
using VOID.VSS.Infrastructure.Configurations.Dapper.Interfaces;

namespace VOID.VSS.Application.Commands.Users;

public class UserCommandHandler(IDapperWrapper dapperWrapper)
{
    public async Task<int> InsertUserAsync(InsertUserCommand command, CancellationToken ct)
    {
        DynamicParameters parameters = new();
        var query = $"""
                        INSERT INTO stock_members ("userId", "role", "active")
                        VALUES (@userId, @role, @active);
                     """;
        parameters.AddDynamicParams(new
        {
            Id = command.UserId,
            role = "Membro",
            active = command.IsActive
        });

        await dapperWrapper.ExecuteQuery(EDatabase.Postgres, query, ct, parameters);
        return 200;

    }
}