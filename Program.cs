using Desafio_Kinetic.Services;
using Hangfire;
using Hangfire.MemoryStorage;
using Serilog;
using Desafio_Kinetic.Dashboard;


var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(new Serilog.Formatting.Compact.RenderedCompactJsonFormatter())
    .WriteTo.File(new Serilog.Formatting.Compact.RenderedCompactJsonFormatter(), "Logs/log.json", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Debug()
    .CreateLogger();

builder.Host.UseSerilog();

// Configurar Hangfire
builder.Services.AddHangfire(config =>
    config.UseMemoryStorage()); // Para pruebas, podés usar UseSqlServerStorage() o Redis

builder.Services.AddHangfireServer();
// Configurar persistencia
builder.Services.AddSingleton<ProcessStateStore>();
builder.Services.AddSingleton<GenDocs>();
// Configurar Procesamiento
builder.Services.AddTransient<FolderProcessor>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<FolderProcessor>>();
    var genDocs = sp.GetRequiredService<GenDocs>();
    var states = sp.GetRequiredService<ProcessStateStore>();
    return new FolderProcessor(logger, genDocs,states, "/app/output");
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(80); // escucha en 0.0.0.0:80
});
builder.Services.AddAuthorization();
builder.Services.AddLogging();



var app = builder.Build();

// Permite Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Permite redirección HTTP → HTTPS si aplica
app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();
app.UseRouting();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllDashboardAuthorizationFilter() }
});

app.MapControllers();

app.Run();




