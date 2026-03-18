using System.Data;
using System.Data.Common;
using VOID.VSS.Infrastructure.Configurations.Dapper.Enum;

namespace VOID.VSS.Infrastructure.Configurations.Dapper.Interfaces;

/// <summary>
/// Interface defining the contract for a Dapper wrapper, providing methods for database operations such as executing queries, managing transactions, and handling bulk inserts. This interface abstracts the underlying database interactions, allowing for easier testing and maintenance of data access logic.
/// </summary>
public interface IDapperWrapper
{
    /// <summary>
    /// Method to begin a unit of work, which typically involves starting a database transaction. This allows multiple database operations to be executed as a single unit, ensuring that either all operations succeed or all fail together, maintaining data integrity.
    /// </summary>
    /// <param name="database"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task BeginUnitOfWorkAsync(EDatabase database, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Method to commit a unit of work, which typically involves committing a database transaction. This finalizes all operations performed within the transaction, making the changes permanent in the database. If any operation within the transaction fails, the entire transaction can be rolled back to maintain data integrity.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CommitUnitOfWorkAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Method to roll back a unit of work, which typically involves rolling back a database transaction. This undoes all operations performed within the transaction, reverting the database to its previous state before the transaction began. This is crucial for maintaining data integrity in case of errors or exceptions during the transaction.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RollbackUnitOfWorkAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Method to create a new database with the specified name. This is useful for scenarios where dynamic database creation is required, such as in multi-tenant applications or during testing. The method takes an EDatabase enum value to specify which database to create, along with the desired name for the new database and a cancellation token for managing asynchronous operations.
    /// </summary>
    /// <param name="database"></param>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateDatabase(EDatabase database, string name, CancellationToken cancellationToken);
    
    /// <summary>
    /// Method to create a database if it does not already exist. This is a common operation in scenarios where the application needs to ensure that a specific database is available before performing any operations on it. The method takes an EDatabase enum value to specify which database to check and create if necessary.
    /// </summary>
    /// <param name="database"></param>
    /// <returns></returns>
    Task CreateDatabaseIfDontExists(EDatabase database);

    /// <summary>
    /// Method to execute a bulk insert operation with merge functionality. This method allows for efficient insertion of large amounts of data into a database table, while also providing the ability to merge existing records based on specified criteria. The method takes a DataTable containing the data to be inserted, the name of the target table, a query for merging records, batch size for processing the data in chunks, and a timeout value for the operation. This is particularly useful for scenarios where data needs to be updated or inserted based on certain conditions, such as when synchronizing data from an external source or performing upserts.
    /// </summary>
    /// <param name="dataTable"></param>
    /// <param name="queryTable"></param>
    /// <param name="queryMerge"></param>
    /// <param name="batchSize"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    Task ExecuteBulkInsertWithMergeAsync(DataTable dataTable, string queryTable, string queryMerge, int batchSize = 100_000, int timeout = 480);
    
    /// <summary>
    /// Method to execute a bulk insert operation. This method allows for efficient insertion of large amounts of data into a database table. The method takes the name of the target database, the name of the target table, and a DataTable containing the data to be inserted. This is particularly useful for scenarios where large datasets need to be inserted into the database in a performant manner, such as during data migration or when importing data from external sources.
    /// </summary>
    /// <param name="database"></param>
    /// <param name="table"></param>
    /// <param name="dataTable"></param>
    /// <returns></returns>
    Task ExecuteBulkInsert(string database, string table, DataTable dataTable);

    /// <summary>
    /// Method to retrieve records from the database based on a specified SQL command. This method allows for executing a query and mapping the results to a collection of objects of type T. The method takes an EDatabase enum value to specify which database to query, the SQL command to execute, optional parameters for the query, and a flag indicating whether to use a transaction for the operation. This is useful for scenarios where data needs to be retrieved from the database and mapped to strongly-typed objects for further processing in the application.
    /// </summary>
    /// <param name="database"></param>
    /// <param name="sqlCommand"></param>
    /// <param name="parameters"></param>
    /// <param name="useTransaction"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IEnumerable<T> GetRecords<T>(EDatabase database, string sqlCommand, object? parameters = null, bool useTransaction = false);
    
    /// <summary>
    /// Method to asynchronously retrieve records from the database based on a specified SQL command. This method allows for executing a query and mapping the results to a collection of objects of type T in an asynchronous manner. The method takes an EDatabase enum value to specify which database to query, the SQL command to execute, a cancellation token for managing asynchronous operations, optional parameters for the query, and flags indicating whether to dispose of resources after execution and whether to use a transaction for the operation. This is useful for scenarios where data needs to be retrieved from the database without blocking the calling thread, allowing for improved performance and responsiveness in applications that require concurrent operations or handle large datasets.
    /// </summary>
    /// <param name="database"></param>
    /// <param name="sqlCommand"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="parameters"></param>
    /// <param name="shouldDispose"></param>
    /// <param name="useTransaction"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<IEnumerable<T>> GetRecordsAsync<T>(EDatabase database, string sqlCommand, CancellationToken cancellationToken, object? parameters = null, bool shouldDispose = true, bool useTransaction = false);

    /// <summary>
    /// Method to retrieve a single record from the database based on a specified SQL command. This method allows for executing a query and mapping the result to an object of type T. The method takes an EDatabase enum value to specify which database to query, the SQL command to execute, optional parameters for the query, and a flag indicating whether to use a transaction for the operation. This is useful for scenarios where a single record needs to be retrieved from the database and mapped to a strongly-typed object for further processing in the application.
    /// </summary>
    /// <param name="database"></param>
    /// <param name="sqlCommand"></param>
    /// <param name="parameters"></param>
    /// <param name="useTransaction"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T? GetRecord<T>(EDatabase database, string sqlCommand, object? parameters = null, bool useTransaction = false);
    
    /// <summary>
    /// Method to asynchronously retrieve a single record from the database based on a specified SQL command. This method allows for executing a query and mapping the result to an object of type T in an asynchronous manner. The method takes an EDatabase enum value to specify which database to query, the SQL command to execute, a cancellation token for managing asynchronous operations, optional parameters for the query, and flags indicating whether to dispose of resources after execution and whether to use a transaction for the operation. This is useful for scenarios where a single record needs to be retrieved from the database without blocking the calling thread, allowing for improved performance and responsiveness in applications that require concurrent operations or handle large datasets.
    /// </summary>
    /// <param name="database"></param>
    /// <param name="sqlCommand"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="parameters"></param>
    /// <param name="shouldDispose"></param>
    /// <param name="useTransaction"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<T?> GetRecordAsync<T>(EDatabase database, string sqlCommand, CancellationToken cancellationToken, object? parameters = null, bool shouldDispose = true, bool useTransaction = false);

    /// <summary>
    /// Method to asynchronously retrieve records from the database as a stream based on a specified SQL command. This method allows for executing a query and mapping the results to a stream of objects of type T in an asynchronous manner. The method takes an EDatabase enum value to specify which database to query, the SQL command to execute, a cancellation token for managing asynchronous operations, and optional parameters for the query. This is useful for scenarios where data needs to be retrieved from the database in a streaming fashion, allowing for improved performance and reduced memory usage when handling large datasets or when processing data in real-time applications.
    /// </summary>
    /// <param name="database"></param>
    /// <param name="sqlCommand"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="parameters"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<Stream> GetRecordsAsyncStream<T>(EDatabase database, string sqlCommand, CancellationToken cancellationToken, object? parameters = null);
    
    /// <summary>
    /// Method to execute a non-query SQL command asynchronously. This method allows for executing commands that do not return any data, such as INSERT, UPDATE, DELETE, or DDL statements. The method takes an EDatabase enum value to specify which database to execute the command against, the SQL command to execute, a cancellation token for managing asynchronous operations, optional parameters for the command, and flags indicating whether to use a transaction for the operation and whether to dispose of resources after execution. This is useful for scenarios where changes need to be made to the database without retrieving any data, allowing for improved performance and responsiveness in applications that require concurrent operations or handle large datasets.
    /// </summary>
    /// <param name="database"></param>
    /// <param name="sqlCommand"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="parameters"></param>
    /// <param name="useTransaction"></param>
    /// <param name="shouldDispose"></param>
    /// <returns></returns>
    Task ExecuteQuery(EDatabase database, string sqlCommand, CancellationToken cancellationToken, object? parameters = null, bool useTransaction = true, bool shouldDispose = true);
    
    /// <summary>
    /// Method to dispose of database resources asynchronously. This method is responsible for cleaning up any resources associated with the database connection, such as closing the connection and disposing of any related objects. The method takes a DbConnection object as a parameter, which represents the database connection to be disposed. This is important for ensuring that database connections are properly managed and released, preventing resource leaks and ensuring optimal performance in applications that interact with databases.
    /// </summary>
    /// <param name="connection"></param>
    /// <returns></returns>
    Task DisposeAsync(DbConnection connection);
    
    /// <summary>
    /// Method to change the name of the database being used by the Dapper wrapper. This allows for dynamic switching between different databases at runtime, which can be useful in scenarios such as multi-tenant applications or when working with multiple databases within the same application. The method takes a string parameter representing the new name of the database to be used. This is important for ensuring that the Dapper wrapper can adapt to different database contexts as needed, providing flexibility in data access operations.
    /// </summary>
    /// <param name="databaseName"></param>
    void ChangeDatabaseName(string databaseName);
    
    /// <summary>
    /// Method to change the name of the database back to its original name. This is useful for scenarios where the Dapper wrapper has been switched to a different database and needs to be reverted back to the original database context. This method does not take any parameters, as it simply resets the database name to its initial value that was configured when the Dapper wrapper was created. This is important for ensuring that the Dapper wrapper can easily switch back to its original database context when needed, providing flexibility in data access operations while maintaining a reference to the initial configuration.
    /// </summary>
    void ChangeDatabaseNameToOriginal();
    
    /// <summary>
    /// Method to retrieve the current connection string being used by the Dapper wrapper. This allows for accessing the connection string for informational purposes or for use in other parts of the application that may require it. The method returns a string representing the current connection string, which can be useful for debugging, logging, or when dynamically constructing database connections in other components of the application. This is important for providing visibility into the database connection configuration and ensuring that the correct connection string is being used for data access operations.
    /// </summary>
    /// <returns></returns>
    string GetConnectionString();
    
    /// <summary>
    /// Method to retrieve the name of the current database being used by the Dapper wrapper. This allows for accessing the database name for informational purposes or for use in other parts of the application that may require it. The method returns a string representing the current database name, which can be useful for debugging, logging, or when dynamically constructing database connections in other components of the application. This is important for providing visibility into the database context and ensuring that the correct database is being used for data access operations.
    /// </summary>
    /// <returns></returns>
    string GetDatabaseName();
}