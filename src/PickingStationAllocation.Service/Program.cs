
using Contracts;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BatchesCreatedConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", h => { });
        cfg.ReceiveEndpoint("station.batches", e => e.ConfigureConsumer<BatchesCreatedConsumer>(context));
    });
});

var app = builder.Build();
app.MapGet("/health", () => "ok");
app.Run("http://0.0.0.0:8080");

class BatchesCreatedConsumer : IConsumer<BatchesCreated>
{
    public async Task Consume(ConsumeContext<BatchesCreated> context)
    {
        var msg = context.Message;
        int stations = Math.Max(1, msg.Params.MaxStationsOpen);
        var assignments = new List<StationAssignment>();
        for (int i = 1; i <= stations; i++)
            assignments.Add(new StationAssignment(i, new List<Guid>()));

        int cursor = 0;
        foreach (var b in msg.Batches)
        {
            assignments[cursor % stations].BatchIds.Add(b.BatchId);
            cursor++;
        }

        await context.Publish(new StationsAllocated(msg.RunId, assignments, msg.Params));
    }
}
