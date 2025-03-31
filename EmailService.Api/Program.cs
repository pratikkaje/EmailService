using EmailService.Application.Interfaces;
using EmailService.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration
var config = builder.Configuration;
builder.Services.Configure<OutlookEmailService>(config.GetSection("Outlook"));

// Dependency Injection
// Change to OutlookEmailService if needed
builder.Services.AddScoped<IEmailService, SendGridEmailService>(); 

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
