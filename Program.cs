using Backend_sec_dev.API.Middleware;
using Backend_sec_dev.Application.Services;
using Serilog;
using Backend_sec_dev.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Host.UseSerilog();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();
builder.Services.AddApplicationServices();
builder.Services.AddDatabase(builder.Configuration);

var app = builder.Build();
builder.AddSerilogLogging();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

app.UseRateLimiter();

app.UseMiddleware<AuthenticationMiddleware>();

app.UseAuthorization();



app.MapControllers();

app.Run();
