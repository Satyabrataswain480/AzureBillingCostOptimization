using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Net;

public class QueryProxy
{
    private readonly CosmosClient _cosmosClient;

    public QueryProxy(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    [Function("QueryProxy")]
    public async Task<IActionResult> GetRecord(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req,
        ILogger logger)
    {
        var id = req.ReadFromJsonAsync<RecordRequest>().Result.Id;
        var hotContainer = _cosmosClient.GetContainer("HotDB", "HotContainer");
        var coldContainer = _cosmosClient.GetContainer("ColdDB", "ColdContainer");

        try
        {
            var record = await hotContainer.ReadItemAsync<BillingRecord>(id, new PartitionKey(id));
            return new OkObjectResult(record);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            var record = await coldContainer.ReadItemAsync<BillingRecord>(id, new PartitionKey(id));
            return new OkObjectResult(record);
        }
    }
}

public class RecordRequest
{
    public string Id { get; set; }
}

public class BillingRecord
{
    public string Id { get; set; }
    public string Timestamp { get; set; }
    // Other properties...
}
