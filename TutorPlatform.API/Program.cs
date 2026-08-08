using TutorPlatform.API;
using TutorPlatform.Application;
using TutorPlatform.Infrastructure;
using TutorPlatform.API.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VNPAY.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

builder.Services.AddVnpayClient(options =>
{
    var section = builder.Configuration.GetSection("Vnpay");
    options.TmnCode = section["TmnCode"] ?? "A5OLM2K8";
    options.HashSecret = section["HashSecret"] ?? "XTCVNPVOBWNZMKKQGPGQZMLPGRDYOWOW";
    options.BaseUrl = section["BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    options.CallbackUrl = section["CallbackUrl"] ?? "http://localhost:5000/api/Credits/vnpay-callback";
    options.Version = "2.1.0";
    options.OrderType = "other";
});

// Add Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DisplayRequestDuration();
});

// Disabled HttpsRedirection to prevent HTTP 307 redirects from stripping Authorization Bearer tokens in local dev
// app.UseHttpsRedirection();

// Use Exception Handler
app.UseExceptionHandler();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TutorPlatform.API.Hubs.NotificationHub>("/hubs/notifications");

// Seed Initial Data (Admin, Subjects & Sample Tutors)
TutorPlatform.Infrastructure.Persistence.DbInitializer.SeedData(app.Services);

app.Run();
