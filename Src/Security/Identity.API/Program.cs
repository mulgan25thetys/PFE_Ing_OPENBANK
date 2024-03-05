using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Identity.API.Applications.Data;
using Identity.API.Applications.Security;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Identity.API.Extensions;
using Identity.API.Services.Interfaces;
using Identity.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//
builder.Services.AddTransient<ISenderService, SenderService>();

//Configuration du context avec la chaine de connection
builder.Services.AddDbContext<IdentityContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AuthConnectionString"));
});

//configuration de Identity User
builder.Services.AddDefaultIdentity<IdentityUser>(config =>
{
    config.SignIn.RequireConfirmedEmail = false;
    config.SignIn.RequireConfirmedAccount = true;

}).AddRoles<IdentityRole>()
  .AddRoleManager<RoleManager<IdentityRole>>()
  .AddUserManager<UserManager<IdentityUser>>()
  .AddDefaultTokenProviders()
  .AddSignInManager()
  .AddEntityFrameworkStores<IdentityContext>();

//Configuration de la partie JWT Tokens
builder.Services.AddCustomAuthentication(builder.Configuration);


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
                     IndexFormat = "Auth.api-logs-" +
                     $"{context.HostingEnvironment.EnvironmentName?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
                     NumberOfReplicas = 1,
                     NumberOfShards = 2,
                 })
                 .Enrich.WithProperty("Environnement", context.HostingEnvironment.EnvironmentName)
                 .ReadFrom.Configuration(context.Configuration);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();

//migration automatique vers la base de donnees
app.MigrateDatabase<IdentityContext>((context, services) =>
{
});

app.MapControllers();

app.Run();
