using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TutorMatchingPlatform.Application;
using TutorMatchingPlatform.Infrastructure;
using TutorMatchingPlatform.Infrastructure.Data;
using TutorMatchingPlatform.API;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddDbContext<TutorMatchingPlatformDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddHostedService<TutorMatchingPlatform.API.BackgroundServices.LateCancellationFlagWorker>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<TutorMatchingPlatformDbContext>();
        var passwordHasher = services.GetRequiredService<TutorMatchingPlatform.Application.Interfaces.Authentication.IPasswordHasher>();
        await TutorMatchingPlatform.Infrastructure.Data.DataSeeder.SeedDataAsync(context, passwordHasher);
    }
    catch (System.Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
