using SqlAnalyzer.Api.Monitoring.BackgroundServices;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<TempFilesMonitoringBackgroundService>();
builder.Services.AddScoped<IMonitoringService>();
builder.Services.AddScoped<IAnalyzeService>();

builder.Logging.AddConsole();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();

app.Run();

