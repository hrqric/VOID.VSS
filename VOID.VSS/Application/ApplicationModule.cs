using VOID.VSS.Application.Commands.Components.Stock;

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
            .AddScoped<ComponentCommandHandler>();
        return services;
    }

}