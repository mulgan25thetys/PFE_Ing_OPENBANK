using Account.API.EventBusConsumer;
using Account.API.Services;
using Account.API.Services.Grpc;
using Account.API.Services.Interfaces;
using EventBus.Message.Common;
using MassTransit;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Helper.Extensions;
using Helper.Middlewares;
using User.grpc.Protos;
using Bank.grpc.Protos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ApplyGdpr();

// Add services to the container.
builder.Services.AddScoped<BankService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddHttpClient<IAccountService, AccountService>(c =>
                c.BaseAddress = new Uri(builder.Configuration["OracleSettings:OrdsDatabaseUrl"]));

builder.Services.AddGrpcClient<UserProtoService.UserProtoServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:UserUrl"]);
});
builder.Services.AddGrpcClient<BankProtoService.BankProtoServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:BankUrl"]);
});

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<TransactionConsumer>();
    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["EventBusSettings:HostAddress"]);
        cfg.ReceiveEndpoint(EventBusConstants.TransactionQueue, c =>
        {
            c.ConfigureConsumer<TransactionConsumer>(ctx);
        });
    });
});

builder.Services.AddMassTransitHostedService();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configuration of Serilog (ELK)
builder.Host.UseSerilog((context, configuration) =>
{
    configuration.Enrich.FromLogContext()
                 .Enrich.WithMachineName()
                 .WriteTo.Console()
                 .WriteTo.Debug()
                 .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(context.Configuration["ElasticConfiguration:Uri"]))
                 {
                     AutoRegisterTemplate = true,
                     IndexFormat = "Account.api-logs-" +
                     $"{context.HostingEnvironment.EnvironmentName?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
                     NumberOfReplicas = 1,
                     NumberOfShards = 2,
                 })
                 .Enrich.WithProperty("Environnement", context.HostingEnvironment.EnvironmentName)
                 .ReadFrom.Configuration(context.Configuration);
});

#region Configuration of Authorization with JWT
builder.Services.AddJwtAuthentication(builder.Configuration);
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.UseMiddleware<JwtMiddleware>();

app.UseCookiePolicy();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
