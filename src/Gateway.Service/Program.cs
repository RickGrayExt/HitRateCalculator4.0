var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.MapGet("/health", () => "ok");
app.MapReverseProxy();
app.Run("http://0.0.0.0:8080");