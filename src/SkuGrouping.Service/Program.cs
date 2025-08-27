
using Contracts;
using MassTransit;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SalesPatternsConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", h => { });
        cfg.ReceiveEndpoint("skugrouping.salespatterns", e => e.ConfigureConsumer<SalesPatternsConsumer>(context));
    });
});

var app = builder.Build();
app.MapGet("/health", () => "ok");
app.Run("http://0.0.0.0:8080");

class SalesPatternsConsumer : IConsumer<SalesPatternsIdentified>
{
    public async Task Consume(ConsumeContext<SalesPatternsIdentified> context)
    {
        var msg = context.Message;
        var groups = msg.Demand
            .GroupBy(d => d.Category)
            .Select(g => new SkuGroup(g.Key, g.Select(x => x.SkuId).ToList()))
            .ToList();

        await context.Publish(new SkuGroupsCreated(msg.RunId, groups, msg.Demand, msg.Params));
    }
}
