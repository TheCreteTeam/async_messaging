using System.Reflection;
using Azure.Identity;
using Coravel;
using Coravel.Events.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;
using WebApplication1.Data;
using WebApplication1.Handlers;
using WebApplication1.Services;
using WebApplication1.Services.Definitions;
using WebApplication1.Validation;
using WebApplication1.WolverineDemo;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
// Add no authentication
builder.Services.AddControllers();

/*var dataProtectDirPath = Environment.GetEnvironmentVariable("ASPNETCORE_DATA_PROTECTION_KEY_PATH");
if (dataProtectDirPath != null)
{
    File system
    builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(dataProtectDirPath));
}
else
{
    builder.Services.AddDataProtection();
}*/

// Key vault
bool devMode = Environment.GetEnvironmentVariable("DEVELOPER_MODE") == "true";
if (devMode)
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(@"/root/ASP.NET/DataProtection-Keys/"));
}
else
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(@"/root/ASP.NET/DataProtection-Keys/"))
        // set life time
        .SetDefaultKeyLifetime(TimeSpan.FromDays(365))
        .ProtectKeysWithAzureKeyVault(
            new Uri("https://keyvaulttesteiopa.vault.azure.net/keys/rsaTestKey/b3b1f20245e54da28519620975b19343"),
            new DefaultAzureCredential());
}

builder.Services.AddBrighter(options =>
{
    var retryPolicy = Policy.Handle<Exception>().WaitAndRetry(new[] 
    { 
        TimeSpan.FromMilliseconds(50), 
        TimeSpan.FromMilliseconds(100), 
        TimeSpan.FromMilliseconds(150) });
    
    var circuitBreakerPolicy = Policy.Handle<Exception>().CircuitBreaker(1, 
        TimeSpan.FromMilliseconds(500));
    
    var retryPolicyAsync = Policy.Handle<Exception>()
        .WaitAndRetryAsync(new[] { TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(150) });
    
    var circuitBreakerPolicyAsync = Policy.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromMilliseconds(500));
    

    var policyRegistry = new PolicyRegistry()
    {
        { CommandProcessor.RETRYPOLICY, retryPolicy },
        { CommandProcessor.CIRCUITBREAKER, circuitBreakerPolicy },
        { CommandProcessor.RETRYPOLICYASYNC, retryPolicyAsync },
        { CommandProcessor.CIRCUITBREAKERASYNC, circuitBreakerPolicyAsync}
    };
    options.PolicyRegistry = policyRegistry;
    options.HandlerLifetime = ServiceLifetime.Scoped;
    options.CommandProcessorLifetime = ServiceLifetime.Scoped;
    options.MapperLifetime = ServiceLifetime.Singleton;
    options.TransformerLifetime = ServiceLifetime.Scoped;
    options.RequestContextFactory= new InMemoryRequestContextFactory();
}).AutoFromAssemblies(typeof(GetMessageHandler).Assembly);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddEvents();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0.3",
        Title = "Test API",
        Description = "A test ASP.NET Core Web API",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });
});

// Logging  .NET 6 Middleware
builder.Services.AddHttpLogging(logging =>
{
    // Customize HTTP logging here.
    logging.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath |
                            HttpLoggingFields.ResponseStatusCode;
    logging.RequestHeaders.Add("sec-ch-ua");
    logging.ResponseHeaders.Add("my-response-header");
    logging.MediaTypeOptions.AddText("application/javascript");
    // logging.RequestBodyLogLimit = 4096;
    // logging.ResponseBodyLogLimit = 4096;
});

/*
 // Permanent Https redirection
 if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
        options.HttpsPort = 443;
    });
}*/

builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(60);
});

builder.Services.AddHttpContextAccessor();


// Services
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<PostListener>();
// Coravel Scheduler
builder.Services.AddScheduler();
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
    
    // add db bus
    // x.AddStateObserver<RegistrationStateMachine, RegistrationState, MessageConsumerDefinition>()
    //     .EntityFrameworkRepository(r =>
    //     {
    //         r.ExistingDbContext<ApplicationDbContext>();
    //         r.UsePostgres();
    //     });
    
    // x.UsingInMemory((context, cfg) =>
    // {
    //     // other options in ConfigureConsumer class
    //     cfg.ConfigureEndpoints(context);
    // });
    
    // The outbox can also be added to all consumers using a configure endpoints callback
    // x.AddConfigureEndpointsCallback((context, name, cfg) =>
    // {
    //     cfg.UseEntityFrameworkOutbox<ApplicationDbContext>(context);
    // });
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
builder.Services.AddPostgresMigrationHostedService(create: true, delete: false);


// Wolverine
builder.Host.UseWolverine();

// Initialise DB context
builder.Services.AddTransient<DbInitialiser>();


var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseMiddleware<ValidationExceptionMiddleware>();
app.UseHttpLogging();

// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();


var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application started.");
string asciiArt = @"
  _   _      _      _____                                      
 | \ | |    | |    / ____|                   /\                
 |  \| | ___| |_  | |     ___  _ __ ___     /  \   _ __  _ __  
 | . ` |/ _ \ __| | |    / _ \| '__/ _ \   / /\ \ | '_ \| '_ \ 
 | |\  |  __/ |_  | |___| (_) | | |  __/  / ____ \| |_) | |_) |
 |_| \_|\___|\__|  \_____\___/|_|  \___| /_/    \_\ .__/| .__/ 
                                                  | |   | |    
                                                  |_|   |_|    
    ";
logger.LogInformation(asciiArt);

var sdkVersion = System.Reflection.Assembly
    .GetEntryAssembly()?
    .GetCustomAttribute<System.Reflection.AssemblyFileVersionAttribute>()?
    .Version;

logger.LogInformation($"SDK Version: {sdkVersion}");


var testConfVar = Environment.GetEnvironmentVariable("TEST_CONF_VAR");
logger.LogInformation($"TEST_CONF_VAR: {testConfVar}");

app.UseHsts();
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    bool useHeaderSts = Environment.GetEnvironmentVariable("SET_HEADER_STS") == "true";
    if (useHeaderSts)
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000");
    await next();
});
 
app.Services.UseScheduler(scheduler =>
{
    // Sample schedule
    //     scheduler.Schedule(
    //         () => Console.WriteLine("Every minute during the week.")
    //     )
    //     .EveryMinute()
    //     .Weekday();

    // Default every 1 minutes produce a message 
    var msgPublishCron = Environment.GetEnvironmentVariable("TEST_MESSAGE_PUBLISH_CRON");
    scheduler.ScheduleWithParams<BgPublisher>()
        .Cron(msgPublishCron ?? "*/1 * * * *");
});

// Brighter
var provider = app.Services;
IEventRegistration registration = provider.ConfigureEvents();
registration
    .Register<PostCreated>()
    .Subscribe<PostListener>();

// Initialise DB
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var initialiser = services.GetRequiredService<DbInitialiser>();
initialiser.Run();

app.Run();