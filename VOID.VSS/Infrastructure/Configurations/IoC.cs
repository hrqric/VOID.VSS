using VOID.VSS.Infrastructure.Configurations.Dapper.Interfaces;
using VOID.VSS.Infrastructure.Configurations.Dapper.Services;
using VOID.VSS.Infrastructure.Configurations.Dapper.Interfaces;

namespace VOID.VSS.Infrastructure.Configurations;

public static class IoC
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging();
        
        var serviceProvider = services.BuildServiceProvider();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        Console.WriteLine($"ConnectionString: {connectionString}");

        services.AddScoped<IDapperWrapper>(provider =>
        {
            return new DapperWrapper(connectionString!, provider.GetService<ILogger<DapperWrapper>>()!);
        });

        services.AddHttpClient("default", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "VOID.VSS");
            client.Timeout = Timeout.InfiniteTimeSpan;
        });
        
        return services;
    }
}