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
            public static readonly string ConnectionString = "AccountEndpoint=https://cosmos-startours-sql.documents.azure.com:443/;AccountKey=i7mhimUqkmvknhxaw2knLSaF5ujoDS7xVdeM6wkPzq6kQ0BpBGBX8qcrSPw4ItPG9A6u2RaqKTPRE9Jn93NmOg==;";// Environment.GetEnvironmentVariable("STAR_TOURS_CONNECTION_STRING_COSMOS_DB");

            public const string DatabaseId = "cosmos-startours-sql-db";

            public static class Containers
            {
                public const string Streams = "streams";
                public const string Leases = "leases";
                public const string Views = "views";
            }

            public static class Gremlin
            {
                public const string HostName = "cosmos-startours-gremlin.gremlin.cosmos.azure.com";

                public static readonly string AuthorizationKey = "8v4Z4RsFk0fQbZA4yTLeF0ffq4mKRqgCJoZHRFk4WI8UffUCfk4bo5g1GTPk8FhfJefT8toFdZ6gmPaqjQRl3A==";// Environment.GetEnvironmentVariable("STAR_TOURS_GREMLIN_AUTHKEY");

                public const string DatabaseId = "cosmos-startours-gremlin-db";

                public const string GraphId = "flights";
            }
        }
    }
}
