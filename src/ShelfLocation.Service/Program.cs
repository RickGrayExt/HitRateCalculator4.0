
using Contracts;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SkuGroupsConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", h => { });
        cfg.ReceiveEndpoint("shelflocation.skugroups", e => e.ConfigureConsumer<SkuGroupsConsumer>(context));
    });
});

var app = builder.Build();
app.MapGet("/health", () => "ok");
app.Run("http://0.0.0.0:8080");

class SkuGroupsConsumer : IConsumer<SkuGroupsCreated>
{
    public async Task Consume(ConsumeContext<SkuGroupsCreated> context)
    {
        var msg = context.Message;
        int perRack = Math.Max(1, msg.Params.MaxSkusPerRack);
        var allSkus = msg.Groups.SelectMany(g => g.Skus).ToList();

        var locations = new List<ShelfLocation>();
        int rackId = 1; int shelf = 1; int pos = 1; int countInRack = 0;

        foreach (var sku in allSkus)
        {
            if (countInRack >= perRack)
            {
                rackId++; shelf = 1; pos = 1; countInRack = 0;
            }
            locations.Add(new ShelfLocation(sku, rackId, shelf, pos));
            countInRack++; pos++;
            if (pos > 10) { pos = 1; shelf++; }
        }

        await context.Publish(new ShelfLocationsAssigned(msg.RunId, locations, msg.Demand, msg.Params));
    }
}
