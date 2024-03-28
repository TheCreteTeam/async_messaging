using Azure.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.OpenApi.Models;
using WebApplication1.Services;
using WebApplication1.Services.Definitions;
using WebApplication1.Validation;

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

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0.8",
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

app.UseHsts();
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000");
    await next();
});
// app.UseHttpsRedirection();
app.Run();