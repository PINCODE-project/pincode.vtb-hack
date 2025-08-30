using SqlAnalyzer.Api.Monitoring.BackgroundServices;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHostedService<TempFilesMonitoringBackgroundService>();
builder.Services.AddScoped<IMonitoringService>();

var app = builder.Build();
app.Run();

