using Player.Application;
using Player.Infrastructure;
using Player.Infrastructure.Persistence.Seeding;
using Player.Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureApi();

builder.Services.AddInfrastructureServices(builder.Configuration).AddApplicationServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.Services.MigrateAndSeedDevelopmentDataAsync();
}

app.ConfigureStart();

await app.RunAsync();
