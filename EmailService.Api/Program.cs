using EmailService.Api.Middlewares;
using EmailService.Application.Interfaces;
using EmailService.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    //.WriteTo.Seq("http://localhost:5341") // Example for Seq, a structured log server
);

// Load configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

if (!builder.Environment.IsDevelopment())
{
    // Load secrets from environment variables (set by GitHub Actions)
    builder.Configuration["SendGrid:ApiKey"] = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
    builder.Configuration["SendGrid:FromEmail"] = Environment.GetEnvironmentVariable("SENDGRID_FROMEMAIL");
    builder.Configuration["Outlook:Email"] = Environment.GetEnvironmentVariable("OUTLOOK_EMAIL");
    builder.Configuration["Outlook:ClientId"] = Environment.GetEnvironmentVariable("OUTLOOK_CLIENTID");
    builder.Configuration["Outlook:TenantId"] = Environment.GetEnvironmentVariable("OUTLOOK_TENANTID");
    builder.Configuration["Outlook:ClientSecret"] = Environment.GetEnvironmentVariable("OUTLOOK_CLIENTSECRET");
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();

// Change to OutlookEmailService if needed
builder.Services.AddScoped<IEmailService, SendGridEmailService>();
//builder.Services.AddScoped<IEmailService,OutlookEmailService>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

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
