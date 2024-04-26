using System.Reflection;
using MassTransit;
using WebApplication2.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Postgres
builder.Services.AddDbContext<ApplicationDbContext>();

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    // x.SetInMemorySagaRepositoryProvider();

    var entryAssembly = Assembly.GetEntryAssembly();
    
    x.AddConsumers(entryAssembly);
    // x.AddSagaStateMachines(entryAssembly);
    // x.AddSagas(entryAssembly);
    x.AddActivities(entryAssembly);
    
    // https://masstransit.io/documentation/configuration/middleware/outbox
    x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
    {
        // o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
        o.QueryDelay = TimeSpan.FromSeconds(15);
        o.UsePostgres();
        // o.UseSqlServer();
        // enable the bus outbox
        o.UseBusOutbox();
    });
    
    x.UsingPostgres((context, cfg) =>
    {
        cfg.UseDbMessageScheduler();
    
        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddOptions<SqlTransportOptions>().Configure(options =>
{
    options.Host = "host.docker.internal";
    options.Database = "AsyncMessageDb";
    options.Schema = "transport"; // the schema for the transport-related tables, etc. 
    options.Role = "transport";   // the role to assign for all created tables, functions, etc.
    options.Username = "postgres";  // the application-level credentials to use
    options.Password = "jijikos";
    options.AdminUsername = "postgres"; // the admin credentials to create the tables, etc.
    options.AdminPassword = "jijikos";
});
var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        // log call
        app.Logger.LogInformation("GetWeatherForecast called");
        
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}