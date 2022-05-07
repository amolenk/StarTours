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
                public const string ConnectionString = "AccountEndpoint=https://cosmos-startours-gremlin.documents.azure.com:443/;AccountKey=JVgf4AdAxYmzEHUelxMBHxFvIKjSuZLTNJchGIuT8HCEM60Mk8GjOoeByBrVaUTrlO1ahvcyfQUS8emp9GFS5Q==;ApiKind=Gremlin;";
            }
        }
    }
}
