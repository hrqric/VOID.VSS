namespace VOID.VSS.Infrastructure.Configurations.Dapper.Enum;

/// <summary>
/// Database type enumeration representing supported database systems, such as SQL Server and PostgreSQL. This enum can be used to specify the type of database being used in various operations, such as connection management, query execution, or database-specific logic handling.
/// </summary>
public enum EDatabase
{
    /// <summary>
    /// Represents the SQL Server database system, which is a relational database management system developed by Microsoft. It is widely used for enterprise applications and supports a wide range of features for data storage, retrieval, and management.
    /// </summary>
    SqlServer,

    /// <summary>
    /// Represents the PostgreSQL database system, which is an open-source relational database management system known for its robustness, extensibility, and support for advanced features. It is widely used in various applications and industries for its reliability and performance.
    /// </summary>
    Postgres
}