
using Contracts;
using MassTransit;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ShelfLocationsConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", h => { });
        cfg.ReceiveEndpoint("rackcalc.locations", e => e.ConfigureConsumer<ShelfLocationsConsumer>(context));
    });
});

var app = builder.Build();
app.MapGet("/health", () => "ok");
app.Run("http://0.0.0.0:8080");

class ShelfLocationsConsumer : IConsumer<ShelfLocationsAssigned>
{
    public async Task Consume(ConsumeContext<ShelfLocationsAssigned> context)
    {
        var msg = context.Message;
        var racks = msg.Locations.GroupBy(l => l.RackId)
            .Select(g => new Rack(g.Key, g.Count(), g.Select(x => x.SkuId).Distinct().ToList()))
            .OrderBy(r => r.RackId)
            .ToList();

        await context.Publish(new RackLayoutCalculated(msg.RunId, racks, msg.Locations, msg.Demand, msg.Params));
    }
}
