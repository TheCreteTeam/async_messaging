using System.Net;

namespace WebApplication1.Validation; 
using FluentValidation;

public class ValidationExceptionMiddleware
{
    private readonly RequestDelegate _request;
    private readonly ILogger<ValidationExceptionMiddleware> _logger;

    public ValidationExceptionMiddleware(RequestDelegate request, ILogger<ValidationExceptionMiddleware> logger)
    {
        _request = request;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _request(context);
        }
        catch (ValidationException exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            var messages = exception.Errors.Select(x => x.ErrorMessage).ToList();
            var validationFailureResponse = new ValidationFailureResponse
            {
                Errors = messages
            };
            await context.Response.WriteAsJsonAsync(validationFailureResponse);
        }
        catch (Exception e)
        {
            _logger.LogError("Exception error: {}", e.ToString());
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
    }
}
