using Gremlin.Net.Driver;
using StarTours.Shared;
using StarTours.Shared.Gremlin;

namespace StarTours.Scenarios;

public class SC004A_SeedData
{
    public async Task RunAsync()
    {
        using var gremlinClient = GremlinClientFactory.CreateClient(
            Config.CosmosDb.Gremlin.HostName,
            Config.CosmosDb.Gremlin.AuthorizationKey,
            Config.CosmosDb.Gremlin.DatabaseId,
            Config.CosmosDb.Gremlin.GraphId);

        var requests = new[]
        {
            "g.V().drop()",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'thx1138').property('name', 'Spaceport THX1138')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'alderaan').property('name', 'Alderaan').property('climate', 'temperate').property('gravity', '1 standard').property('terrain', 'grasslands, mountains')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'yaviniv').property('name', 'Yavin IV').property('climate', 'temperate, tropical').property('gravity', '1 standard').property('terrain', 'jungle, rainforests')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'hoth').property('name', 'Hoth').property('climate', 'frozen').property('gravity', '1.1 standard').property('terrain', 'tundra, ice caves, mountain ranges')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'dagobah').property('name', 'Dagobah').property('climate', 'murky').property('gravity', 'N/A').property('terrain', 'swamp, jungles')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'bespin').property('name', 'Bespin').property('climate', 'temperate').property('gravity', '1.5 (surface), 1 standard (Cloud City)').property('terrain', 'gas giant')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'endor').property('name', 'Endor').property('climate', 'temperate').property('gravity', '0.85 standard').property('terrain', 'forests, mountains, lakes')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'naboo').property('name', 'Naboo').property('climate', 'temperate').property('gravity', '1 standard').property('terrain', 'grassy hills, swamps, forests, mountains')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'coruscant').property('name', 'Coruscant').property('climate', 'temperate').property('gravity', '1 standard').property('terrain', 'cityscape, mountains')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'kamino').property('name', 'Kamino').property('climate', 'temperate').property('gravity', '1 standard').property('terrain', 'ocean')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'geonosis').property('name', 'Geonosis').property('climate', 'temperate, arid').property('gravity', '0.9 standard').property('terrain', 'rock, desert, mountain, barren')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'utapau').property('name', 'Utapau').property('climate', 'temperate, arid, windy').property('gravity', '1 standard').property('terrain', 'scrublands, savanna, canyons, sinkholes')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'mustafar').property('name', 'Mustafar').property('climate', 'hot').property('gravity', '1 standard').property('terrain', 'volcanoes, lava rivers, mountains, caves')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'kashyyyk').property('name', 'Kashyyyk').property('climate', 'tropical').property('gravity', '1 standard').property('terrain', 'jungle, forests, lakes, rivers')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'polismassa').property('name', 'Polis Massa').property('climate', 'artificial temperate ').property('gravity', '0.56 standard').property('terrain', 'airless asteroid')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'mygeeto').property('name', 'Mygeeto').property('climate', 'frigid').property('gravity', '1 standard').property('terrain', 'glaciers, mountains, ice canyons')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'felucia').property('name', 'Felucia').property('climate', 'hot, humid').property('gravity', '0.75 standard').property('terrain', 'fungus forests')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'catoneimoidia').property('name', 'Cato Neimoidia').property('climate', 'temperate, moist').property('gravity', '1 standard').property('terrain', 'mountains, fields, forests, rock arches')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'saleucami').property('name', 'Saleucami').property('climate', 'hot').property('terrain', 'caves, desert, mountains, volcanoes')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'stewjon').property('name', 'Stewjon').property('climate', 'temperate').property('gravity', '1 standard').property('terrain', 'grass')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'eriadu').property('name', 'Eriadu').property('climate', 'polluted').property('gravity', '1 standard').property('terrain', 'cityscape')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'corellia').property('name', 'Corellia').property('climate', 'temperate').property('gravity', '1 standard').property('terrain', 'plains, urban, hills, forests')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'rodia').property('name', 'Rodia').property('climate', 'hot').property('gravity', '1 standard').property('terrain', 'jungles, oceans, urban, swamps')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'nalhutta').property('name', 'Nal Hutta').property('climate', 'temperate').property('gravity', '1 standard').property('terrain', 'urban, oceans, swamps, bogs')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'dantooine').property('name', 'Dantooine').property('climate', 'temperate').property('gravity', '1 standard').property('terrain', 'oceans, savannas, mountains, grasslands')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'bestineiv').property('name', 'Bestine IV').property('climate', 'temperate').property('terrain', 'rocky islands, oceans')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'ordmantell').property('name', 'Ord Mantell').property('climate', 'temperate').property('gravity', '1 standard').property('terrain', 'plains, seas, mesas')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'trandosha').property('name', 'Trandosha').property('climate', 'arid').property('gravity', '0.62 standard').property('terrain', 'mountains, seas, grasslands, deserts')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'socorro').property('name', 'Socorro').property('climate', 'arid').property('gravity', '1 standard').property('terrain', 'deserts, mountains')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'moncala').property('name', 'Mon Cala').property('climate', 'temperate').property('gravity', '1').property('terrain', 'oceans, reefs, islands')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'chandrila').property('name', 'Chandrila').property('climate', 'temperate').property('gravity', '1').property('terrain', 'plains, forests')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'sullust').property('name', 'Sullust').property('climate', 'superheated').property('gravity', '1').property('terrain', 'mountains, volcanoes, rocky deserts')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'toydaria').property('name', 'Toydaria').property('climate', 'temperate').property('gravity', '1').property('terrain', 'swamps, lakes')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'malastare').property('name', 'Malastare').property('climate', 'arid, temperate, tropical').property('gravity', '1.56').property('terrain', 'swamps, deserts, jungles, mountains')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'dathomir').property('name', 'Dathomir').property('climate', 'temperate').property('gravity', '0.9').property('terrain', 'forests, deserts, savannas')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'ryloth').property('name', 'Ryloth').property('climate', 'temperate, arid, subartic').property('gravity', '1').property('terrain', 'mountains, valleys, deserts, tundra')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'aleenminor').property('name', 'Aleen Minor')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'vulpter').property('name', 'Vulpter').property('climate', 'temperate, artic').property('gravity', '1').property('terrain', 'urban, barren')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'troiken').property('name', 'Troiken').property('terrain', 'desert, tundra, rainforests, mountains')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'tund').property('name', 'Tund').property('terrain', 'barren, ash')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'haruunkal').property('name', 'Haruun Kal').property('climate', 'temperate').property('gravity', '0.98').property('terrain', 'toxic cloudsea, plateaus, volcanoes')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'cerea').property('name', 'Cerea').property('climate', 'temperate').property('gravity', '1').property('terrain', 'verdant')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'gleeanselm').property('name', 'Glee Anselm').property('climate', 'tropical, temperate').property('gravity', '1').property('terrain', 'lakes, islands, swamps, seas')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'iridonia').property('name', 'Iridonia').property('terrain', 'rocky canyons, acid pools')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'tholoth').property('name', 'Tholoth')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'iktotch').property('name', 'Iktotch').property('climate', 'arid, rocky, windy').property('gravity', '1').property('terrain', 'rocky')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'quermia').property('name', 'Quermia')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'dorin').property('name', 'Dorin').property('climate', 'temperate').property('gravity', '1')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'champala').property('name', 'Champala').property('climate', 'temperate').property('gravity', '1').property('terrain', 'oceans, rainforests, plateaus')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'mirial').property('name', 'Mirial').property('terrain', 'deserts')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'serenno').property('name', 'Serenno').property('terrain', 'rainforests, rivers, mountains')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'concorddawn').property('name', 'Concord Dawn').property('terrain', 'jungles, forests, deserts')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'zolan').property('name', 'Zolan')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'ojom').property('name', 'Ojom').property('climate', 'frigid').property('terrain', 'oceans, glaciers')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'skako').property('name', 'Skako').property('climate', 'temperate').property('gravity', '1').property('terrain', 'urban, vines')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'muunilinst').property('name', 'Muunilinst').property('climate', 'temperate').property('gravity', '1').property('terrain', 'plains, forests, hills, mountains')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'shili').property('name', 'Shili').property('climate', 'temperate').property('gravity', '1').property('terrain', 'cities, savannahs, seas, plains')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'kalee').property('name', 'Kalee').property('climate', 'arid, temperate, tropical').property('gravity', '1').property('terrain', 'rainforests, cliffs, canyons, seas')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'umbara').property('name', 'Umbara')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'tatooine').property('name', 'Tatooine').property('climate', 'arid').property('gravity', '1 standard').property('terrain', 'desert')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'jakku').property('name', 'Jakku').property('terrain', 'deserts')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'kessel').property('name', 'Kessel').property('climate', 'cold and dry with thin atmosphere, occasionally smoggy').property('gravity', '0.82 standard').property('terrain', 'barren rocky mountains')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'gorse').property('name', 'Gorse').property('climate', 'tidally-locked').property('gravity', '1 standard')",
            "g.addV('terminal').property('partitionKey', 'map').property('id', 'cynda').property('name', 'Cynda').property('terrain', 'barren rocky mountains')"
        };

        foreach (string request in requests)
        {
            await gremlinClient.SubmitAsync(request);
        }
    }
}
