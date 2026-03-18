using System.Text;
using VOID.VSS.Infrastructure.Configurations.Dapper.Interfaces;
using VOID.VSS.Infrastructure.Configurations.Dapper.Enum;

namespace VOID.VSS.Infrastructure.Configurations.Dapper.Utils;

/// <summary>
/// Manages database migrations, including checking for table existence and creating tables as needed using provided SQL scripts.
/// </summary>
public class MigrationManager
{
    private int _tries;
    private List<string> _retryTableCreation = new();
    private string _scriptsPath = "";
    private readonly IDapperWrapper dapperWrapper;
    private readonly ILogger<MigrationManager> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationManager"/> class.
    /// </summary>
    /// <param name="dapperWrapper">The Dapper wrapper for database operations.</param>
    /// <param name="logger">The logger for logging migration operations.</param>
    public MigrationManager(IDapperWrapper dapperWrapper, ILogger<MigrationManager> logger)
    {
        this.dapperWrapper = dapperWrapper;
        this.logger = logger;
    }

    /// <summary>
    /// Checks if the required tables exist in the database and creates them if they do not exist.
    /// </summary>
    /// <param name="layerName">The name of the application layer (used to locate scripts).</param>
    /// <param name="database">The target database type.</param>
    /// <param name="currentDirectory">The current working directory.</param>
    public async Task CheckIfTablesExists(string layerName, EDatabase database, string currentDirectory)
    {
        await dapperWrapper.CreateDatabaseIfDontExists(database);

        logger.LogInformation("Checking Tables");

        SetScriptsPath(layerName, currentDirectory);
        var query = BuildTableExistenceCheck(database);

        var lstTables = await dapperWrapper.GetRecordsAsync<(string Name, bool Exist)>(database, query, CancellationToken.None);

        foreach (var (name, exist) in lstTables)
        {
            if (!exist)
                await CreateTable(name, database);
        }
    }

    /// <summary>
    /// Creates a table in the database using the corresponding SQL script. Retries creation if it fails, up to a maximum number of attempts.
    /// </summary>
    /// <param name="tableName">The name of the table to create.</param>
    /// <param name="database">The target database type.</param>
    private async Task CreateTable(string tableName, EDatabase database)
    {
        try
        {
            logger.LogInformation($"Creating Table: {tableName}");
            await dapperWrapper.ExecuteQuery(database, File.ReadAllText(Path.Combine(_scriptsPath, $"{tableName}.sql"), Encoding.Latin1), CancellationToken.None);
            logger.LogInformation($"Successfully Created\n\n");

            while (_retryTableCreation.Any())
            {
                string table = _retryTableCreation.First();
                logger.LogInformation($"Re-Trying to Create Table: {table}");
                await dapperWrapper.ExecuteQuery(database, File.ReadAllText(Path.Combine(_scriptsPath, $"{table}.sql"), Encoding.Latin1), CancellationToken.None);
                logger.LogInformation($"Successfully Created\n\n");
                _retryTableCreation.Remove(table);
            }
        }
        catch (Exception e)
        {
            if (_tries >= 20)
            {
                logger.LogError($"MaxValue for trying to create table reached\nError: {e.Message}\nStack: {e.StackTrace}");
                throw;
            }

            _tries++;
            _retryTableCreation.Add(tableName);
        }
    }

    /// <summary>
    /// Builds a SQL query to check for the existence of required tables based on available scripts and the database type.
    /// </summary>
    /// <param name="database">The target database type.</param>
    /// <returns>A SQL query string to check table existence.</returns>
    private string BuildTableExistenceCheck(EDatabase database)
    {
        string filePath = Path.Combine(_scriptsPath, "CreateTablesOrder.txt");
        List<string> sqlFiles;
        
        if (File.Exists(filePath))
        {
            List<string> order = File.ReadAllText(filePath, Encoding.Latin1).Split("\n").Select(x => x.Replace("\r", "")).ToList();

            sqlFiles = Directory.GetFiles(_scriptsPath, "*.sql")
                .Select(Path.GetFileName).OrderBy(item => order.IndexOf(item!)).ToList()!;
        }
        else
        {
            sqlFiles = Directory.GetFiles(_scriptsPath, "*.sql")
                .Select(Path.GetFileName)
                .ToList()!;
        }


        List<string> tablesToCreate = new();

        foreach (var sqlFile in sqlFiles)
        {
            string tableName = sqlFile.Replace(".sql", "");
            tableName = $"\'{tableName}'";

            if (database == EDatabase.Postgres)
                tablesToCreate.Add($"SELECT {tableName}, EXISTS(SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = {tableName}) AS {tableName.Replace("\'", "")}Table");

            else
                tablesToCreate.Add($"SELECT {tableName}, CASE WHEN OBJECT_ID ({tableName}, 'U') IS NOT NULL THEN 1 ELSE CASE WHEN OBJECT_ID ({tableName}, 'TR') IS NOT NULL THEN 1 ELSE 0 END END AS {tableName.Replace("\'", "")}Table");
        }

        return $@"{string.Join(" UNION ALL ", tablesToCreate)}";
    }

    /// <summary>
    /// Sets the path to the SQL scripts directory based on the layer name and current directory.
    /// </summary>
    /// <param name="layerName">The name of the application layer.</param>
    /// <param name="currentDirectory">The current working directory.</param>
    private void SetScriptsPath(string layerName, string currentDirectory)
    {
        if (string.IsNullOrWhiteSpace(currentDirectory) || string.IsNullOrWhiteSpace(layerName))
        {
            _scriptsPath = "Scripts";
        }
        else
        {
            string parentDirectory = Path.GetDirectoryName(currentDirectory)!;

            _scriptsPath = Path.Combine(parentDirectory, layerName, "Scripts");
        }

    }
}