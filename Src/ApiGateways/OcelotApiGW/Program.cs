using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Configuration;
using Ocelot.Cache.CacheManager;
using Ocelot.Values;
using System.Text;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOcelot()
    .AddCacheManager(settings => settings.WithDictionaryHandle());

builder.WebHost
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        config.AddJsonFile($"ocelot.{hostContext.HostingEnvironment.EnvironmentName}.json", true, true);
    })

    .ConfigureLogging((hostingContext, loggingBuilder) =>
    {
        loggingBuilder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
        loggingBuilder.AddConsole();
        loggingBuilder.AddDebug();
    });
var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/", async context =>
    {
        await context.Response.WriteAsync("Hello World!");
    });
});
await app.UseOcelot();
app.Run();
