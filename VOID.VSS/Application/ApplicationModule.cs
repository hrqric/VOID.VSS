using VOID.VSS.Application.Commands.Address;
using VOID.VSS.Application.Commands.Components.Stock;
using VOID.VSS.Application.Queries;

namespace VOID.VSS.Application;

public static class ApplicationModule
{
    public static IServiceCollection ConfigureApplication(this IServiceCollection services)
    {
        services.ConfigureHandlers();
        return services;
    }

    public static IServiceCollection ConfigureHandlers(this IServiceCollection services)
    {
        services
            .AddScoped<ComponentCommandHandler>()
            .AddScoped<AddressCommandHandler>()
            .AddScoped<ComponentQueryHandler>()
            ;
        return services;
    }

}