using System.Data;
using System.Data.Common;
using System.Text.Json;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.SqlClient;
using Npgsql;
using VOID.VSS.Infrastructure.Configurations.Dapper.Interfaces;
using VOID.VSS.Infrastructure.Configurations.Dapper.Enum;

namespace VOID.VSS.Infrastructure.Configurations.Dapper.Services;

/// <summary>
/// Implementation of a Dapper wrapper, providing methods for database operations such as executing queries, managing transactions, and handling bulk inserts. This class abstracts the underlying database interactions, allowing for easier testing and maintenance of data access logic.
/// </summary>
public class DapperWrapper : IDapperWrapper
{
    private string _connectionString;
    private readonly string _elasticPool = "";
    private string _databaseName;
    private readonly string _originalDatabaseName;
    private readonly ILogger<DapperWrapper> _logger;

    #region UnitOfWork

    private DbConnection? _unitOfWorkConnection;
    private DbTransaction? _unitOfWorkTransaction;

    private bool IsInUnitOfWork => _unitOfWorkTransaction != null;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="DapperWrapper"/> class with the specified connection string and logger.
    /// </summary>
    /// <param name="conn">The database connection string.</param>
    /// <param name="logger">The logger instance for logging operations.</param>
    public DapperWrapper(string conn, ILogger<DapperWrapper> logger)
    {
        _logger = logger;
        _connectionString = conn;

        _databaseName = GetDatabaseName();
        _originalDatabaseName = _databaseName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DapperWrapper"/> class with the specified connection string, elastic pool, and logger.
    /// </summary>
    /// <param name="conn">The database connection string.</param>
    /// <param name="elasticPool">The name of the elastic pool for SQL Server.</param>
    /// <param name="logger">The logger instance for logging operations.</param>
    public DapperWrapper(string conn, string elasticPool, ILogger<DapperWrapper> logger) : this(conn, logger)
    {
        _elasticPool = elasticPool;
    }

    #region Common

    public DbTransaction? Transaction { get; set; }

    #region Async Methods

    /// <inheritdoc/>
    public async Task<T?> GetRecordAsync<T>(EDatabase database, string sqlCommand,
        CancellationToken cancellationToken = default, object? parameters = null, bool shouldDispose = true,
        bool useTransaction = false)
    {
        T? record;

        DbConnection connection = null!;
        bool ownsConnection = false;
        bool beganLocalTransaction = false;
        try
        {
            if (IsInUnitOfWork)
            {
                connection = _unitOfWorkConnection!;
                Transaction = _unitOfWorkTransaction!;
            }
            else
            {
                connection = SetConnectionType(database);
                ownsConnection = true;
                
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync(cancellationToken);
                
                if (useTransaction)
                {
                    Transaction = await BeginTransactionAsync(connection);
                    beganLocalTransaction = true;
                }
            }

            record = await connection.QueryFirstOrDefaultAsync<T>(sqlCommand, parameters, Transaction);
            
            if (beganLocalTransaction)
                await Transaction!.CommitAsync(cancellationToken);
        }
        catch (SqlException e) when (e.Number == 4060)
        {
            if (beganLocalTransaction)
                await Transaction!.RollbackAsync(cancellationToken);
            
            _logger.LogError(
                $"Cannot open database. Error: {e.Message}\n\nDatabase: {GetDatabaseName()}Stack: {e.StackTrace}");
            throw new HttpRequestException(
                "Could not access the database. Please check your credentials and try again.");
        }
        catch (Exception e)
        {
            if (beganLocalTransaction)
                await Transaction!.RollbackAsync(cancellationToken);
            
            _logger.LogError(
                $"An error happened while trying to retrieve record\nError: {e.Message}\nDatabase: {GetDatabaseName()}\nStack: {e.StackTrace}");
            throw new Exception("An error happened while trying to retrieve record", e);
        }
        finally
        {
            if (ownsConnection && shouldDispose)
            {
                await connection.CloseAsync();
                await connection.DisposeAsync();
            }
        }

        return record;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> GetRecordsAsync<T>(EDatabase database, string sqlCommand,
        CancellationToken cancellationToken = default, object? parameters = null, bool shouldDispose = true,
        bool useTransaction = false)
    {
        IEnumerable<T> records;

        DbConnection connection = null!;
        bool ownsConnection = false;
        bool beganLocalTransaction = false;
        try
        {
            if (IsInUnitOfWork)
            {
                connection = _unitOfWorkConnection!;
                Transaction = _unitOfWorkTransaction!;
            }
            else
            {
                connection = SetConnectionType(database);
                ownsConnection = true;
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync(cancellationToken);
                if (useTransaction)
                {
                    Transaction = await connection.BeginTransactionAsync(cancellationToken);
                    beganLocalTransaction = true;
                }
            }

            records = await connection.QueryAsync<T>(sqlCommand, parameters, Transaction);

            if (beganLocalTransaction)
                await Transaction!.CommitAsync(cancellationToken);
        }
        catch (SqlException e) when (e.Number == 4060)
        {
            if (beganLocalTransaction)
                await Transaction!.RollbackAsync(cancellationToken);
            
            _logger.LogError(
                $"Cannot open database. Error: {e.Message}\nDatabase: {GetDatabaseName()}\nStack: {e.StackTrace}");
            throw new HttpRequestException(
                "Could not access the database. Please check your credentials and try again.");
        }
        catch (Exception e)
        {
            if (beganLocalTransaction)
                await Transaction!.RollbackAsync(cancellationToken);
            
            _logger.LogError(
                $"An error happened while trying to retrieve records\nError: {e.Message}\nDatabase: {GetDatabaseName()}\nStack: {e.StackTrace}");
            throw new Exception(
                "An error happened while trying to retrieve record. Please contact support for further information, using the correlation ID found in the response header 'BDSH-Correlation-ID'.",
                e);
        }
        finally
        {
            if (ownsConnection)
            {
                if (shouldDispose)
                {
                    await connection.CloseAsync();
                    await connection.DisposeAsync();
                }
            }
        }

        return records;
    }

    /// <inheritdoc/>
    public async Task<Stream> GetRecordsAsyncStream<T>(EDatabase database, string sqlCommand, CancellationToken cancellationToken, object? parameters = null)
    {
        DbConnection connection = null!;
        bool ownsConnection = false;
        
        try
        {
            if (IsInUnitOfWork)
            {
                connection = _unitOfWorkConnection!;
            }
            else
            {
                connection = SetConnectionType(database);
                ownsConnection = true;
                await connection.OpenAsync(cancellationToken);
            }

            var data = await connection.QueryAsync<T>(sqlCommand, parameters,
                IsInUnitOfWork ? _unitOfWorkTransaction : null);

            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, data, cancellationToken: cancellationToken);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
        catch (SqlException e) when (e.Number == 4060)
        {
            _logger.LogError(
                $"Cannot open database. Error: {e.Message}\nDatabase: {GetDatabaseName()}\nStack: {e.StackTrace}");
            throw new HttpRequestException(
                "Could not access the database. Please check your credentials and try again.");
        }
        catch (Exception e)
        {
            _logger.LogError(
                $"An error happened while trying to retrieve records\nError: {e.Message}\nDatabase: {GetDatabaseName()}\nStack: {e.StackTrace}");
            throw;
        }
        finally
        {
            if (ownsConnection)
            {
                await connection.CloseAsync();
                await connection.DisposeAsync();
            }
        }
    }

    /// <inheritdoc/>
    public async Task ExecuteQuery(EDatabase database, string sqlCommand, CancellationToken cancellationToken,
        object? parameters = null, bool useTransaction = true, bool shouldDispose = true)
    {
        DbConnection connection = null!;
        bool ownsConnection = false;
        bool beganLocalTransaction = false;
        
        try
        {
            if (IsInUnitOfWork)
            {
                connection = _unitOfWorkConnection!;
                Transaction = _unitOfWorkTransaction!;
                await connection.ExecuteAsync(sqlCommand, parameters, Transaction);
            }
            else
            {
                connection = SetConnectionType(database);
                ownsConnection = true;
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync(cancellationToken);

                if (useTransaction)
                {
                    Transaction = await BeginTransactionAsync(connection);
                    beganLocalTransaction = true;
                    await connection.ExecuteAsync(sqlCommand, parameters, Transaction);
                    await Transaction.CommitAsync(cancellationToken);
                }
                else
                {
                    await connection.ExecuteAsync(sqlCommand, parameters);
                }
            }
        }
        catch (SqlException e) when (e.Number == 4060)
        {
            if (beganLocalTransaction)
                await Transaction!.RollbackAsync(cancellationToken);
            
            _logger.LogError(
                $"Cannot open database. Error: {e.Message}\nDatabase: {GetDatabaseName()}\nStack: {e.StackTrace}");
            throw new HttpRequestException(
                "Could not access the database. Please check your credentials and try again.");
        }
        catch (Exception e)
        {
            if (beganLocalTransaction)
                await Transaction!.RollbackAsync(cancellationToken);
            
            _logger.LogError(
                $"An error happened while trying to execute query, rolling back query!\nError: {e.Message}\nDatabase: {GetDatabaseName()}\nStack: {e.StackTrace}");
            throw new Exception("An error happened while trying to execute query", e);
        }
        finally
        {
            if (ownsConnection)
            {
                await connection.CloseAsync();
                if (useTransaction && beganLocalTransaction && Transaction != null)
                    await Transaction.DisposeAsync();
                if (shouldDispose)
                    await connection.DisposeAsync();
            }
        }
    }

    /// <inheritdoc/>
    public async Task ExecuteBulkInsertWithMergeAsync(DataTable dataTable, string queryTable, string queryMerge,
        int batchSize = 100_000, int timeout = 480)
    {
        SqlConnection? conn = null;
        SqlTransaction? localTransaction = null;
        
        bool useLocalTransaction = false;

        try
        {
            if (IsInUnitOfWork)
            {
                if (_unitOfWorkConnection is not SqlConnection sqlConn ||
                    _unitOfWorkTransaction is not SqlTransaction sqlTran)
                    throw new NotSupportedException(
                        "Bulk operations are only supported for SQL Server when using UnitOfWork.");

                conn = sqlConn;

                await using (var command = conn.CreateCommand())
                {
                    command.Transaction = sqlTran;
                    command.CommandText = queryTable;
                    command.CommandTimeout = timeout;
                    await command.ExecuteNonQueryAsync();
                }

                using (var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, sqlTran))
                {
                    bulk.DestinationTableName = dataTable.TableName;
                    bulk.BatchSize = batchSize;
                    bulk.BulkCopyTimeout = timeout;
                    await bulk.WriteToServerAsync(dataTable);
                }

                await using (var command = conn.CreateCommand())
                {
                    command.Transaction = sqlTran;
                    command.CommandText = queryMerge;
                    command.CommandTimeout = timeout;
                    await command.ExecuteNonQueryAsync();
                }
            }
            else
            {
                conn = new SqlConnection(_connectionString);
                if (conn.State == ConnectionState.Closed)
                    await conn.OpenAsync();

                localTransaction = (SqlTransaction)await conn.BeginTransactionAsync();
                useLocalTransaction = true;

                await using (var command = conn.CreateCommand())
                {
                    command.Transaction = localTransaction;
                    command.CommandText = queryTable;
                    command.CommandTimeout = timeout;
                    await command.ExecuteNonQueryAsync();
                }

                using (var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, localTransaction))
                {
                    bulk.DestinationTableName = dataTable.TableName;
                    bulk.BatchSize = batchSize;
                    bulk.BulkCopyTimeout = timeout;
                    await bulk.WriteToServerAsync(dataTable);
                }

                await using (var command = conn.CreateCommand())
                {
                    command.Transaction = localTransaction;
                    command.CommandText = queryMerge;
                    command.CommandTimeout = timeout;
                    await command.ExecuteNonQueryAsync();
                }

                await localTransaction.CommitAsync();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(
                $"An error happened while trying to execute bulk insert, rolling back query!\nError: {e.Message}\nDatabase: {GetDatabaseName()}\nStack: {e.StackTrace}");
            if (useLocalTransaction && localTransaction != null)
                await localTransaction.RollbackAsync();
            throw;
        }
        finally
        {
            if (useLocalTransaction && conn != null)
            {
                if (localTransaction != null)
                    await localTransaction.DisposeAsync();
                await conn.CloseAsync();
                await conn.DisposeAsync();
            }
        }
    }

    /// <inheritdoc/>
    public async Task ExecuteBulkInsert(string database, string table, DataTable dataTable)
    {
        if (IsInUnitOfWork)
        {
            if (_unitOfWorkConnection is not SqlConnection sqlConn ||
                _unitOfWorkTransaction is not SqlTransaction sqlTran)
                throw new NotSupportedException(
                    "Bulk operations are only supported for SQL Server when using UnitOfWork.");

            using var bulkCopy = new SqlBulkCopy(sqlConn, SqlBulkCopyOptions.Default, sqlTran);
            bulkCopy.DestinationTableName = table;
            await bulkCopy.WriteToServerAsync(dataTable);
            return;
        }

        string oldConnectionString = _connectionString;
        _connectionString = _connectionString.Replace(_databaseName, database);

        using var bulkCopyOutside = new SqlBulkCopy(_connectionString);
        bulkCopyOutside.DestinationTableName = table;
        await bulkCopyOutside.WriteToServerAsync(dataTable);

        _connectionString = oldConnectionString;
    }

    /// <inheritdoc/>
    public async Task CreateDatabaseIfDontExists(EDatabase database)
    {
        _logger.LogInformation("Checking if database already exists");
        string oldConnectionString = _connectionString;

        _connectionString =
            _connectionString.Replace(_databaseName, database == EDatabase.Postgres ? "postgres" : "master");

        await using (var connection = SetConnectionType(database))
        {
            try
            {
                await connection.OpenAsync();
                string createDatabaseQuery = $"CREATE DATABASE {_databaseName}";
                object parameters = new
                {
                    DatabaseName = _databaseName
                };

                var query = database switch
                {
                    EDatabase.Postgres => "SELECT COUNT(*) FROM pg_database WHERE datname = @DatabaseName",
                    EDatabase.SqlServer => "SELECT COUNT(*) FROM sys.databases WHERE name = @DatabaseName",
                    _ => throw new ArgumentOutOfRangeException(nameof(database), database, null)
                };

                if (await connection.QueryFirstOrDefaultAsync<int>(query, parameters,
                        Transaction) == 0)
                {
                    await ExecuteQuery(database, createDatabaseQuery, CancellationToken.None, useTransaction: false);
                    _logger.LogInformation($"Database created successfully!");
                }
            }
            catch (SqlException e) when (e.Number == 4060)
            {
                _logger.LogError(
                    $"Cannot open database. Error: {e.Message}\nDatabase: {GetDatabaseName()}\nStack: {e.StackTrace}");
                throw new HttpRequestException(
                    "Could not access the database. Please check your credentials and try again.");
            }
            catch (Exception e)
            {
                _logger.LogError(
                    $"An error happened while trying to create database, check stack for more info\nError: {e.Message}\nDatabase: {GetDatabaseName()}\nStack: {e.StackTrace}");
                throw new Exception(
                    "An error happened while trying to create database. Please contact support for further information, using the correlation ID found in the response header 'BDSH-Correlation-ID'. ",
                    e);
            }

            await connection.CloseAsync();
        }

        _connectionString = oldConnectionString;
    }

    /// <inheritdoc/>
    public async Task CreateDatabase(EDatabase database, string name, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking if database already exists");
        string oldConnectionString = _connectionString;

        _connectionString =
            _connectionString.Replace(_databaseName, database == EDatabase.Postgres ? "postgres" : "master");

        await using (var connection = SetConnectionType(database))
        {
            try
            {
                await connection.OpenAsync(cancellationToken);

                var query = database switch
                {
                    EDatabase.Postgres => "SELECT COUNT(*) FROM pg_database WHERE datname = @Name",
                    EDatabase.SqlServer => "SELECT COUNT(*) FROM sys.databases WHERE name = @Name",
                    _ => throw new NotImplementedException()
                };

                if (await connection.QueryFirstOrDefaultAsync<int>(query, new { Name = name }, Transaction) == 0)
                {
                    string createDatabaseQuery =
                        $"CREATE DATABASE [{name}] ( SERVICE_OBJECTIVE = ELASTIC_POOL ( name = [{_elasticPool}] ) )";

                    await ExecuteQuery(database, createDatabaseQuery, cancellationToken, useTransaction: false);
                    _logger.LogInformation($"Database created successfully!");
                }
                else
                {
                    var message = $"Database already exists ({name})";
                    _logger.LogInformation(message);
                    throw new Exception(message);
                }
            }
            catch (SqlException e) when (e.Number == 4060)
            {
                _logger.LogError(
                    $"Cannot create database. Error: {e.Message}\nDatabase: {GetDatabaseName()}\nStack: {e.StackTrace}");
                throw new HttpRequestException(
                    "Could not access the database. Please check your credentials and try again.");
            }
            catch (Exception e)
            {
                _logger.LogError(
                    $"An error happened while trying to create database, check stack for more info\nError: {e.Message}\nDatabase: {GetDatabaseName()}\nStack: {e.StackTrace}");
                throw new Exception(
                    "An error happened while trying to access database. Please contact support for further information, using the correlation ID found in the response header 'BDSH-Correlation-ID'. ",
                    e);
            }

            await connection.CloseAsync();
        }

        _connectionString = oldConnectionString;
    }

    #endregion

    #region Sync Methods

    /// <inheritdoc/>
    public T? GetRecord<T>(EDatabase database, string sqlCommand, object? parameters = null, bool useTransaction = false)
    {
        T? record;

        DbConnection connection = null!;
        bool ownsConnection = false;
        bool beganLocalTransaction = false;
        try
        {
            if (IsInUnitOfWork)
            {
                connection = _unitOfWorkConnection!;
                Transaction = _unitOfWorkTransaction!;
            }
            else
            {
                connection = SetConnectionType(database);
                ownsConnection = true;
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                if (useTransaction)
                {
                    Transaction = connection.BeginTransaction();
                    beganLocalTransaction = true;
                }
            }

            record = connection.QueryFirstOrDefault<T>(sqlCommand, parameters, Transaction);
            
            if (beganLocalTransaction)
                Transaction!.Commit();
        }
        catch (SqlException e) when (e.Number == 4060)
        {
            if (beganLocalTransaction)
                Transaction!.Rollback();
            
            _logger.LogError(
                $"Cannot open database. Error: {e.Message}\nDatabase: {GetDatabaseName()}\nStack: {e.StackTrace}");
            throw new HttpRequestException(
                "Could not access the database. Please check your credentials and try again.");
        }
        catch (Exception e)
        {
            if (beganLocalTransaction)
                Transaction!.Rollback();
            
            _logger.LogError(
                $"An error happened while trying to retrieve record\nError: {e.Message}\nDatabase: {GetDatabaseName()}\nStack: {e.StackTrace}");
            throw new Exception("An error happened while trying to retrieve record", e);
        }
        finally
        {
            if (ownsConnection)
            {
                connection.Close();
                connection.Dispose();
            }
        }

        return record;
    }

    /// <inheritdoc/>
    public IEnumerable<T> GetRecords<T>(EDatabase database, string sqlCommand, object? parameters = null,
        bool useTransaction = false)
    {
        IEnumerable<T> records;

        DbConnection connection = null!;
        bool ownsConnection = false;
        bool beganLocalTransaction = false;
        try
        {
            if (IsInUnitOfWork)
            {
                connection = _unitOfWorkConnection!;
                Transaction = _unitOfWorkTransaction!;
            }
            else
            {
                connection = SetConnectionType(database);
                ownsConnection = true;
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                if (useTransaction)
                {
                    Transaction = connection.BeginTransaction();
                    beganLocalTransaction = true;
                }
            }

            records = connection.Query<T>(sqlCommand, parameters, Transaction);
            
            if (beganLocalTransaction)
                Transaction!.Commit();
        }
        catch (SqlException e) when (e.Number == 4060)
        {
            if (beganLocalTransaction)
                Transaction!.Rollback();
            
            _logger.LogError($"Cannot open database. Error: {e.Message}\nStack: {e.StackTrace}");
            throw new HttpRequestException(
                "Could not access the database. Please check your credentials and try again.");
        }
        catch (Exception e)
        {
            if (beganLocalTransaction)
                Transaction!.Rollback();
            
            _logger.LogError(
                $"An error happened while trying to retrieve records\nError: {e.Message}\nDatabase: {GetDatabaseName()}\nStack: {e.StackTrace}");
            throw new Exception(
                "An error happened while trying to retrieve record. Please contact support for further information, using the correlation ID found in the response header 'BDSH-Correlation-ID'.",
                e);
        }
        finally
        {
            if (ownsConnection)
            {
                connection.Close();
                connection.Dispose();
            }
        }

        return records;
    }

    #endregion

    #region Aux

    async Task<DbTransaction> BeginTransactionAsync(DbConnection connection) =>
        await connection.BeginTransactionAsync();

    /// <inheritdoc/>
    public async Task DisposeAsync(DbConnection connection)
    {
        if (connection.State == ConnectionState.Open)
            await connection.DisposeAsync();
    }

    private DbConnection SetConnectionType(EDatabase database)
    {
        DbConnection connection = database switch
        {
            EDatabase.Postgres => new NpgsqlConnection(_connectionString),
            _ => throw new ArgumentOutOfRangeException(nameof(database), database, null)
        };

        return connection;
    }

    #endregion

    #endregion

    #region UnitOfWork

    /// <inheritdoc/>
    public async Task BeginUnitOfWorkAsync(EDatabase database, CancellationToken cancellationToken = default)
    {
        if (IsInUnitOfWork)
            throw new InvalidOperationException("A UnitOfWork is already active.");

        _unitOfWorkConnection = SetConnectionType(database);
        if (_unitOfWorkConnection.State == ConnectionState.Closed)
            await _unitOfWorkConnection.OpenAsync(cancellationToken);
        _unitOfWorkTransaction = await _unitOfWorkConnection.BeginTransactionAsync(cancellationToken);
        Transaction = _unitOfWorkTransaction; // keep legacy property in sync
    }

    /// <inheritdoc/>
    public async Task CommitUnitOfWorkAsync(CancellationToken cancellationToken = default)
    {
        if (!IsInUnitOfWork)
            return;

        try
        {
            await _unitOfWorkTransaction!.CommitAsync(cancellationToken);
        }
        finally
        {
            await _unitOfWorkTransaction!.DisposeAsync();
            await _unitOfWorkConnection!.CloseAsync();
            await _unitOfWorkConnection.DisposeAsync();
            _unitOfWorkTransaction = null;
            _unitOfWorkConnection = null;
            Transaction = null;
        }
    }

    /// <inheritdoc/>
    public async Task RollbackUnitOfWorkAsync(CancellationToken cancellationToken = default)
    {
        if (!IsInUnitOfWork)
            return;

        try
        {
            await _unitOfWorkTransaction!.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _unitOfWorkTransaction!.DisposeAsync();
            await _unitOfWorkConnection!.CloseAsync();
            await _unitOfWorkConnection.DisposeAsync();
            _unitOfWorkTransaction = null;
            _unitOfWorkConnection = null;
            Transaction = null;
        }
    }

    #endregion

    #region Aux

    /// <inheritdoc/>
    public void ChangeDatabaseName(string databaseName)
    {
        string connectionStringDatabaseName = GetDatabaseName();

        if (!connectionStringDatabaseName.Equals(databaseName))
        {
            _connectionString = _connectionString.Replace(connectionStringDatabaseName, databaseName);

            _databaseName = databaseName;
        }
    }

    /// <inheritdoc/>
    public void ChangeDatabaseNameToOriginal()
    {
        string connectionStringDatabaseName = GetDatabaseName();

        if (!connectionStringDatabaseName.Equals(_originalDatabaseName))
        {
            _connectionString = _connectionString.Replace(connectionStringDatabaseName, _originalDatabaseName);
            _databaseName = _originalDatabaseName;
        }
    }

    /// <inheritdoc/>
    public string GetConnectionString() => _connectionString;

    /// <inheritdoc/>
    public string GetDatabaseName()
    {
        string databaseName = Regex.Match(_connectionString, @"database=(.*?)(;|$)", RegexOptions.IgnoreCase).Groups[1]
            .Value;

        if (string.IsNullOrWhiteSpace(databaseName))
            databaseName = Regex.Match(_connectionString, @"Initial Catalog=(.*?)(;|$)", RegexOptions.IgnoreCase)
                .Groups[1].Value;

        return databaseName;
    }
    
}

    #endregion