﻿using ProjectP.Errors;

namespace ProjectP.Middlewares;
using System.Net;
using System.Text.Json;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware (RequestDelegate next,ILogger<ExceptionMiddleware>logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
      
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

            var response = _env.IsDevelopment()
                ? new ApiException((int)HttpStatusCode.InternalServerError, e.Message, e.StackTrace?.ToString())
                : new ApiException((int)HttpStatusCode.InternalServerError);

            var option = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
            var json = JsonSerializer.Serialize(response,option);

            await context.Response.WriteAsync(json);
        }
    }
}