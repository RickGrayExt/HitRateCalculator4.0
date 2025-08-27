
using Contracts;
using MassTransit;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<StationsAllocatedConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", h => { });
        cfg.ReceiveEndpoint("efficiency.stations", e => e.ConfigureConsumer<StationsAllocatedConsumer>(context));
    });
});

var app = builder.Build();
app.MapGet("/health", () => "ok");
app.Run("http://0.0.0.0:8080");

class StationsAllocatedConsumer : IConsumer<StationsAllocated>
{
    public async Task Consume(ConsumeContext<StationsAllocated> context)
    {
        var msg = context.Message;
        int totalOrders = msg.Assignments.Sum(a => a.BatchIds.Count);
        int singleStationOrders = msg.Assignments.Count(a => a.BatchIds.Count > 0);
        double hitRate = totalOrders == 0 ? 0 : (double)singleStationOrders / totalOrders;

        var result = new HitRateResult(msg.RunId, Math.Round(hitRate, 4), totalOrders, singleStationOrders);
        await context.Publish(new HitRateCalculated(msg.RunId, result));
    }
}
