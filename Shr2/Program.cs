﻿﻿﻿﻿﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Shr2.Interfaces;
using Shr2.Services;
using Shr2.Providers;
using Microsoft.Extensions.Configuration;
using Shr2;
using Shr2.Models;
using Shr2.HealthChecks;
using System;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddScoped<IConfig, JConfig>();
builder.Services.AddScoped<IConverter, ConverterService>();
builder.Services.AddScoped<IStorageProvider, AzTableStorage>();

builder.Services.AddHostedService<Initializer>();

// Add controllers and API explorer for Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add memory cache for performance
builder.Services.AddMemoryCache();

// Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            });
    });
    
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new { error = "Too many requests. Please try again later." });
    };
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<AzureStorageHealthCheck>("azure-storage");

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();
