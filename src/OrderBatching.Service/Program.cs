
using Contracts;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<RackLayoutConsumer>();
    x.AddConsumer<StartRunConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", h => { });
        cfg.ReceiveEndpoint("batch.start", e => e.ConfigureConsumer<StartRunConsumer>(context));
        cfg.ReceiveEndpoint("batch.racklayout", e => e.ConfigureConsumer<RackLayoutConsumer>(context));
    });
});

var app = builder.Build();
app.MapGet("/health", () => "ok");
app.Run("http://0.0.0.0:8080");

class StartRunConsumer : IConsumer<StartRunCommand>
{
    public async Task Consume(ConsumeContext<StartRunCommand> context)
    {
        await Task.CompletedTask;
    }
}

class RackLayoutConsumer : IConsumer<RackLayoutCalculated>
{
    public async Task Consume(ConsumeContext<RackLayoutCalculated> context)
    {
        var msg = context.Message;
        int size = Math.Max(1, msg.Params.OrderBatchSize);
        var batches = new List<Batch>();
        int i = 0;
        var skus = msg.Demand.Select(d => d.SkuId).ToList();
        while (i < skus.Count)
        {
            var chunk = skus.Skip(i).Take(size).ToList();
            batches.Add(new Batch(Guid.NewGuid(), new List<string> { $"O{i+1}" }, chunk));
            i += size;
        }
        await context.Publish(new BatchesCreated(msg.RunId, batches, "default", msg.Params));
    }
}
