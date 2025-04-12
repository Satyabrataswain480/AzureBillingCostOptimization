using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public class Archiver
{
    private readonly CosmosClient _cosmosClient;

    public Archiver(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    [Function("Archiver")]
    public async Task Run(
        [CosmosDBTrigger("HotDB", "HotContainer", LeaseCollectionName = "leases")] IReadOnlyList<Document> changes,
        ILogger logger)
    {
        var coldContainer = _cosmosClient.GetContainer("ColdDB", "ColdContainer");

        foreach (var doc in changes)
        {
            if (DateTime.Parse(doc.GetPropertyValue<string>("timestamp")) < DateTime.UtcNow.AddMonths(-3))
            {
                await coldContainer.UpsertItemAsync(doc);
            }
        }
    }
}
