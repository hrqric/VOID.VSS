using VOID.VSS.Infrastructure.Configurations.Dapper.Interfaces;


namespace VOID.VSS.Infrastructure.Configurations.Dapper.Factory;

public class DatabaseRepositoryFactory(IServiceScopeFactory scopeFactory) : IDatabaseRepositoryFactory
{
    /// <inheritdoc/>
    public IDapperWrapper Create()
    {
        var scope = scopeFactory.CreateScope();
        
        return scope.ServiceProvider.GetRequiredService<IDapperWrapper>();
    }
}