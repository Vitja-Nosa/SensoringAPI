using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SensoringAPI.Controllers;
using SensoringAPI.Data;
using SensoringAPI.Repositories;
using SensoringAPI.Services.Interfaces;
using SensoringAPI.Services;

var builder = WebApplication.CreateBuilder(args);
if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true") {
    Console.WriteLine($"RUNNING IN CONTAINER");
    builder.WebHost.UseUrls("http://0.0.0.0:80");
}

builder.Configuration.AddUserSecrets<Program>();
builder.Configuration.AddEnvironmentVariables();

var sqlConnectionString = builder.Configuration.GetValue<string>("SqlConnectionString");
var sqlConnectionStringFound = !string.IsNullOrWhiteSpace(sqlConnectionString);

Console.WriteLine($"[DEBUG] SqlConnectionString found: {sqlConnectionStringFound}");
if (!sqlConnectionStringFound)
    Console.WriteLine("[DEBUG] SqlConnectionString is null or empty.");
else
    Console.WriteLine($"[DEBUG] SqlConnectionString: {sqlConnectionString}");

builder.Services.AddHttpClient<OpenMeteoWeatherService>();

builder.Services.AddScoped<ITestModeService, TestModeService>();


// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetValue<string>("SqlConnectionString")));

builder.Services.AddControllers();
builder.Services.AddScoped<WasteDetectionRepository>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("PerIpPolicy", config =>
    {
        config.PermitLimit = 5; // max 5 requests
        config.Window = TimeSpan.FromSeconds(1); // per 1 second
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 0;
    });
});

// Automatically Validiate ModelState
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = false; // standaardwaarde
});


var app = builder.Build();

app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/api", (IConfiguration config) =>
{
    return Results.Ok(new
    {
        online = true,
        connectionStringFound = sqlConnectionStringFound
    });
});

app.MapControllers();

app.Run();
