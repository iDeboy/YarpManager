using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace YarpManager.Abstractions;
internal sealed class DbConfigProvider : IProxyConfigProvider {
    public IProxyConfig GetConfig() {
        throw new NotImplementedException();
    }

    private sealed class ProxyConfig : IProxyConfig {
        public required IReadOnlyList<RouteConfig> Routes { get; init; }

        public required IReadOnlyList<ClusterConfig> Clusters { get; init; }

        public required IChangeToken ChangeToken { get; init; }
    }
}
