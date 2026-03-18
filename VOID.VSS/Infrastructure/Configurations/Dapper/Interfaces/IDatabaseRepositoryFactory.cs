using VOID.VSS.Infrastructure.Configurations.Dapper.Interfaces;

namespace VOID.VSS.Infrastructure.Configurations.Dapper.Interfaces;

/// <summary>
/// Factory interface for creating instances of IDapperWrapper, which is responsible for managing database connections and executing queries using Dapper. This factory allows for the creation of IDapperWrapper instances with proper scope management, ensuring that resources are handled efficiently and effectively within the application.
/// </summary>
public interface IDatabaseRepositoryFactory
{
    /// <summary>
    /// Creates and returns an instance of IDapperWrapper by utilizing the underlying implementation to manage the scope of dependencies and ensure proper resource handling. This method abstracts the creation logic, allowing for flexibility in how IDapperWrapper instances are instantiated and managed within the application.
    /// </summary>
    /// <returns></returns>
    IDapperWrapper Create();
}