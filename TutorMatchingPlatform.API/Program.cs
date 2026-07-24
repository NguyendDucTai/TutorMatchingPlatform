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
    // Hack to fix Swashbuckle .NET 10 Preview serialization bug for SecurityRequirements
    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/swagger/v1/swagger.json")
        {
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await next();

            context.Response.Body = originalBodyStream;
            responseBody.Seek(0, SeekOrigin.Begin);
            
            var json = await new StreamReader(responseBody).ReadToEndAsync();
            
            // Regex to robustly find the empty security requirement object, regardless of spacing or newlines
            json = System.Text.RegularExpressions.Regex.Replace(
                json, 
                @"""security""\s*:\s*\[\s*\{\s*\}\s*\]", 
                "\"security\": [ { \"Bearer\": [ ] } ]");
                       
            await context.Response.WriteAsync(json);
        }
        else
        {
            await next();
        }
    });

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
