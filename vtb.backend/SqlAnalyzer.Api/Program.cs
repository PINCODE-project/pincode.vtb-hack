using Microsoft.EntityFrameworkCore;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Monitoring.BackgroundServices;
using SqlAnalyzer.Api.Monitoring.Services;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;
using SqlAnalyzer.Api.Services.DbConnection;
using SqlAnalyzer.Api.Services.DbConnection.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))); 


builder.Services.AddHostedService<TempFilesMonitoringBackgroundService>();
builder.Services.AddHostedService<CacheHitMonitoringBackgroundService>();
builder.Services.AddSingleton<IMonitoringService, MonitoringService>();
builder.Services.AddScoped<ITempFilesAnalyzeService, TempFilesTempFilesAnalyzeService>();
builder.Services.AddScoped<ICacheAnalyzeService, CacheAnalyzeService>();
builder.Services.AddScoped<IDbConnectionService, DbConnectionService>();

builder.Logging.AddConsole();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();

    db.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
    await db.Database.MigrateAsync();
}

app.Run();

