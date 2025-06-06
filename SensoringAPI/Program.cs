using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SensoringAPI.Controllers;
using SensoringAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

var sqlConnectionString = builder.Configuration.GetValue<string>("SqlConnectionString");
var sqlConnectionStringFound = !string.IsNullOrWhiteSpace(sqlConnectionString);

// Add services to the container.
builder.Services.AddDbContext<WasteDetectionDBContext>(options =>
options.UseSqlServer(builder.Configuration.GetValue<string>("SqlConnectionString")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Automatically Validiate ModelState
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = false; // standaardwaarde
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
