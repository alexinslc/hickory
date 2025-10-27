using MassTransit;

namespace Hickory.Api.Infrastructure.Messaging;

/// <summary>
/// Configures MassTransit with Redis transport for event-driven messaging
/// </summary>
public static class MassTransitConfiguration
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Register all consumers from the current assembly
            x.AddConsumers(typeof(Program).Assembly);

            // Configure Redis transport
            x.UsingInMemory((context, cfg) =>
            {
                // For development, use in-memory transport
                // In production, switch to Redis:
                // x.UsingRedis((context, cfg) =>
                // {
                //     cfg.Host(configuration["Redis:Host"] ?? "localhost");
                //     cfg.ConfigureEndpoints(context);
                // });
                
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
