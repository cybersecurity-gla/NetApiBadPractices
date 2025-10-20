using System.Net;
using System.Text.Json;
using BadApiExample.DTOs;

namespace BadApiExample.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred. TraceId: {TraceId}", 
            context.TraceIdentifier);

        var response = context.Response;
        response.ContentType = "application/json";

        var apiResponse = new ApiResponseDto<object>
        {
            Success = false,
            Message = "An error occurred while processing your request"
        };

        // Handle different exception types
        if (exception is ArgumentException or ArgumentNullException)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            apiResponse.Message = "Invalid request parameters";
        }
        else if (exception is UnauthorizedAccessException)
        {
            response.StatusCode = (int)HttpStatusCode.Unauthorized;
            apiResponse.Message = "Unauthorized access";
        }
        else if (exception is KeyNotFoundException)
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            apiResponse.Message = "Resource not found";
        }
        else if (exception is InvalidOperationException)
        {
            response.StatusCode = (int)HttpStatusCode.Conflict;
            apiResponse.Message = "Invalid operation";
        }
        else if (exception is TimeoutException)
        {
            response.StatusCode = (int)HttpStatusCode.RequestTimeout;
            apiResponse.Message = "Request timeout";
        }
        else
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            apiResponse.Message = "Internal server error";
        }

        // Include additional details in development environment
        if (_environment.IsDevelopment())
        {
            apiResponse.Errors = new[] 
            { 
                exception.Message,
                exception.StackTrace ?? "No stack trace available"
            };
        }
        else
        {
            // In production, add a unique error ID for tracking
            apiResponse.Errors = new[] { $"Error ID: {context.TraceIdentifier}" };
        }

        var jsonResponse = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await response.WriteAsync(jsonResponse);
    }
}