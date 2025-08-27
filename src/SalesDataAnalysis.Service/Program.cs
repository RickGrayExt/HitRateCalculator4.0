
using Contracts;
using MassTransit;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<StartRunConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", h => { });
        cfg.ReceiveEndpoint("salesdata.start", e => e.ConfigureConsumer<StartRunConsumer>(context));
    });
});

var app = builder.Build();
app.MapGet("/health", () => "ok");
app.Run("http://0.0.0.0:8080");

class StartRunConsumer : IConsumer<StartRunCommand>
{
    public async Task Consume(ConsumeContext<StartRunCommand> context)
    {
        var cmd = context.Message;
        var path = cmd.DatasetPath ?? "/app/data/DataSetClean.csv";
        var list = new List<SkuDemand>();
        try
        {
            if (!File.Exists(path))
            {
                for (int i = 1; i <= 50; i++)
                    list.Add(new SkuDemand($"SKU{i:000}", i * 10, i, Math.Round(i/10.0,2), i%5==0, $"Cat{((i-1)%5)+1}"));
            }
            else
            {
                using var sr = new StreamReader(path);
                string? header = await sr.ReadLineAsync();
                while (!sr.EndOfStream)
                {
                    var line = await sr.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(',');
                    if (parts.Length < 6) continue;
                    var sku = parts[0].Trim();
                    int units = int.TryParse(parts[1], out var u) ? u : 0;
                    int orders = int.TryParse(parts[2], out var o) ? o : 0;
                    double velocity = double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0d;
                    bool seasonal = parts[4].Trim().Equals("true", StringComparison.OrdinalIgnoreCase) || parts[4].Trim() == "1";
                    var category = parts[5].Trim();
                    list.Add(new SkuDemand(sku, units, orders, velocity, seasonal, category));
                }
            }
        }
        catch { }

        await context.Publish(new SalesPatternsIdentified(cmd.RunId, list, cmd.Params));
    }
}
