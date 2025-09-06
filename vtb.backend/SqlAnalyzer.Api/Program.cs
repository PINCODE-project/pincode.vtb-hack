using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SqlAnalyzer.Api.Dal;
using SqlAnalyzer.Api.Monitoring.BackgroundServices;
using SqlAnalyzer.Api.Monitoring.Services;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;
using SqlAnalyzer.Api.Services.DbConnection;
using SqlAnalyzer.Api.Services.DbConnection.Interfaces;
using SqlAnalyzer.Api.Services.LlmClient;
using SqlAnalyzer.Api.Services.LlmClient.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var basePath = AppContext.BaseDirectory;

    var xmlPath = Path.Combine(basePath, "SqlAnalyzer.Api.xml");
    options.IncludeXmlComments(xmlPath, true);

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Postgresql analyzer API",
        Description = "Сервис для анализа метрик бд и sql запросов"
    });
    options.UseInlineDefinitionsForEnums();
});

// Переписываем TargetConnection из DB_DSN при наличии
var dbDsn = builder.Configuration["DB_DSN"];
if (!string.IsNullOrWhiteSpace(dbDsn))
{
    builder.Configuration["ConnectionStrings:TargetConnection"] = dbDsn;
}

builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))); 

var llmBaseUrl = builder.Configuration["LLM_BASE_URL"] ?? "http://5.39.220.103:5009";

builder.Services.AddHttpClient<ILlmClient, LlmClient>(client =>
{
    client.BaseAddress = new Uri(llmBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});



builder.Services.AddHostedService<TempFilesMonitoringBackgroundService>();
builder.Services.AddHostedService<CacheHitMonitoringBackgroundService>();
builder.Services.AddHostedService<AutovacuumBackgroundService>();
builder.Services.AddHostedService<IndexMonitoringBackgroundService>();
builder.Services.AddScoped<IIndexAnalysisService, IndexAnalysisService>();
builder.Services.AddScoped<IPgStatAnalyzerService, PgStatAnalyzerService>();
builder.Services.AddScoped<IMonitoringService, MonitoringService>();
builder.Services.AddScoped<ITempFilesAnalyzeService, TempFilesTempFilesAnalyzeService>();
builder.Services.AddScoped<ICacheAnalyzeService, CacheAnalyzeService>();
builder.Services.AddScoped<IAutovacuumMonitoringService, AutovacuumMonitoringService>();
builder.Services.AddScoped<IAutovacuumAnalysisService, AutovacuumAnalysisService>();
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

