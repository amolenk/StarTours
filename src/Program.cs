using StarTours.Scenarios;

switch (args[0])
{
    case "migrate":
        await new SC00_Migrate().RunAsync();
        break;

    case "aggregate":
        new SC01_FlightAggregate().Run();
        break;

    case "eventstore":
        await new SC02_CosmosEventStore().RunAsync();
        break;

    case "seed":
        await new SC03A_SeedFlights().RunAsync();
        break;

    case "graphproc":
        await new SC03B_FlightGraphProjection().RunAsync();
        break;

    case "snapshotproc":
        await new SC04A_SnapshotProcessor().RunAsync();
        break;

    case "snapshottrigger":
        await new SC04B_SnapshotTrigger().RunAsync();
        break;

    case "snapshotload":
        await new SC04C_SnapshotLoad().RunAsync();
        break;

    default:
        Console.WriteLine("nope :-(");
        break;
}