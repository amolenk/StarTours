using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarTours.Shared
{
    public static class Config
    {
        public static class CosmosDb
        {
            public static readonly string ConnectionString = Environment.GetEnvironmentVariable("STAR_TOURS_CONNECTION_STRING_COSMOS_DB");

            public const string DatabaseId = "cosmosdb-star-tours-db";

            public static class Containers
            {
                public const string Events = "events";
                public const string Leases = "leases";
                public const string Views = "views";
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
}
