using System.Net.WebSockets;
using Gremlin.Net.Driver;
using Gremlin.Net.Structure.IO.GraphSON;

namespace StarTours.Shared.Gremlin;

public static class GremlinClientFactory
{
    public static GremlinClient CreateClient(
        string hostName,
        string authorizationKey,
        string databaseId,
        string graphId)
    {
        var gremlinServer = new GremlinServer(
            hostname: hostName,
            port: 443,
            enableSsl: true,
            username: $"/dbs/{databaseId}/colls/{graphId}",
            password: authorizationKey);

        var connectionPoolSettings = new ConnectionPoolSettings()
        {
            MaxInProcessPerConnection = 10,
            PoolSize = 30, 
            ReconnectionAttempts= 3,
            ReconnectionBaseDelay = TimeSpan.FromMilliseconds(500)
        };

        var webSocketConfiguration =
            new Action<ClientWebSocketOptions>(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(10);
            });

        // For Azure Cosmos DB Gremlin API as of November 2021,
        // it needs to use GraphSON v2.
        return new GremlinClient(
            gremlinServer, 
            new GraphSON2Reader(), 
            new GraphSON2Writer(),
            GremlinClient.GraphSON2MimeType, 
            connectionPoolSettings, 
            webSocketConfiguration);
    }
}