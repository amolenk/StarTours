namespace StarTours.Shared;

public static class Config
{
    public static class CosmosDb
    {
        public static class Sql
        {
            public const string Endpoint = "https://cosmos-startours-sql.documents.azure.com:443/";

            public static readonly string AuthorizationKey = Environment.GetEnvironmentVariable("STAR_TOURS_SQL_AUTHKEY");

            public const string DatabaseId = "cosmos-startours-sql-db";

            public static class Containers
            {
                public const string Streams = "streams";
                public const string Leases = "leases";
            }
        }

        public static class Gremlin
        {
            public const string HostName = "cosmos-startours-gremlin.gremlin.cosmos.azure.com";

            public static readonly string AuthorizationKey = Environment.GetEnvironmentVariable("STAR_TOURS_GREMLIN_AUTHKEY");

            public const string DatabaseId = "cosmos-startours-gremlin-db";

            public const string GraphId = "flights";
        }
    }
}
