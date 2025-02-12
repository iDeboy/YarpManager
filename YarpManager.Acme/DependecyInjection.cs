using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using YarpManager.Acme.Clients;
using YarpManager.Acme.Factories;
using YarpManager.Acme.Handlers;
using YarpManager.Acme.Utils;

// TODO: Implement RFC8555: https://datatracker.ietf.org/doc/html/rfc8555

namespace YarpManager.Acme;
public static class DependecyInjection {

    public static IServiceCollection AddAcmeServices(this IServiceCollection services) {

        //
        // Acme client factory
        //
        services.TryAddTransient<IAcmeClientFactory, DefaultAcmeClientFactory>();
        services.TryAddTransient<IAcmeServiceFactory, DefaultAcmeServiceFactory>();

        // services.AddTransient<IAcmeService, AcmeService>();
        services.AddScoped<AcmeHandler>();
        services.AddHttpClient<IAcmeClient, AcmeClient>();
        services.AddHttpClient(AcmeConstants.HttpClientName, client => {
            client.DefaultRequestVersion = HttpVersion.Version11;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;

            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("YarpManager", Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(".NET", Environment.Version.ToString()));
        })
        .ConfigurePrimaryHttpMessageHandler(() => {
            return new SocketsHttpHandler {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5)
            };
        })
        .SetHandlerLifetime(Timeout.InfiniteTimeSpan)
        //.AddHttpMessageHandler<AcmeHandler>()
        ;

        return services;
    }

}
