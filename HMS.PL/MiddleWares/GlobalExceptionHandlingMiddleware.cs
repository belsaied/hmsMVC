using Microsoft.AspNetCore.Mvc;
using HMS.BLL.Services.Exceptions;

namespace HMS.PL.MiddleWares
{
    public class GlobalExceptionHandlingMiddleware(
        RequestDelegate _next,
        ILogger<GlobalExceptionHandlingMiddleware> _logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
                await HandleNotFoundEndpointAsync(context);
                await HandleForbiddenAsync(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleNotFoundEndpointAsync(HttpContext context)
        {
            if (context.Response.StatusCode == StatusCodes.Status404NotFound
                && !context.Response.HasStarted)
            {
                var response = new ProblemDetails
                {
                    Title = "Endpoint Not Found",
                    Detail = $"The requested endpoint at {context.Request.Path} was not found.",
                    Instance = context.Request.Path,
                    Status = StatusCodes.Status404NotFound
                };
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(response);
            }
        }

        private static async Task HandleForbiddenAsync(HttpContext context)
        {
            if (context.Response.StatusCode == StatusCodes.Status403Forbidden
                && !context.Response.HasStarted)
            {
                var response = new ProblemDetails
                {
                    Title = "Forbidden",
                    Detail = "You do not have permission to access this resource.",
                    Instance = context.Request.Path,
                    Status = StatusCodes.Status403Forbidden
                };
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(response);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
           
            var (statusCode, validationErrors) = ResolveException(ex);

            var response = new ProblemDetails
            {
                Title = GetTitle(ex),
                Detail = ex.Message,
                Instance = context.Request.Path,
                Status = statusCode
            };

            if (validationErrors is not null)
                response.Extensions["errors"] = validationErrors;

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(response);
            }
        }

        private static (int statusCode, IEnumerable<string>? errors) ResolveException(Exception ex) =>
    ex switch
    {
        ValidationException ve => (StatusCodes.Status400BadRequest, ve.Errors),

        NotFoundException => (StatusCodes.Status404NotFound, null),

        UnauthorizedException => (StatusCodes.Status401Unauthorized, null),

        AccountLockedException => (StatusCodes.Status401Unauthorized, null),

        EmailNotVerifiedException
        or ForbiddenException => (StatusCodes.Status403Forbidden, null),

        ConflictException => (StatusCodes.Status409Conflict, null),

        BusinessRuleException => (StatusCodes.Status422UnprocessableEntity, null),

        _ => (StatusCodes.Status500InternalServerError, null)
    };

        private static string GetTitle(Exception ex) => ex switch
        {
            ValidationException => "Validation Error",
            NotFoundException => "Resource Not Found",
            UnauthorizedException => "Unauthorized",
            AccountLockedException => "Account Locked",
            EmailNotVerifiedException => "Email Not Verified",
            ForbiddenException => "Forbidden",
            ConflictException => "Conflict",
            BusinessRuleException => "Business Rule Violation",
            _ => "An unexpected error occurred"
        };
    }

}

