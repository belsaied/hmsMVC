using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HMS.PL.Factories
{
    public class EnumValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var invalidEnums = InvalidEnumTracker.PopInvalid();

            if (invalidEnums is { Count: > 0 })
            {
                foreach (var entry in invalidEnums)
                {
                    context.ModelState.AddModelError(
                        entry.FieldName,
                        $"'{entry.ProvidedValue}' is not a valid value for {entry.FieldName}. " +
                        $"Valid values are: {entry.ValidValues}.");
                }
            }

            if (!context.ModelState.IsValid)
            {
                var acceptsJson = context.HttpContext.Request.Headers.Accept.ToString()
                    .Contains("application/json", StringComparison.OrdinalIgnoreCase);

                if (acceptsJson)
                {
                    var errors = context.ModelState
                        .Where(e => e.Value!.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors
                                .Select(e =>
                                    string.IsNullOrWhiteSpace(e.ErrorMessage)
                                        ? "Invalid value provided."
                                        : e.ErrorMessage)
                        )
                        .ToArray();

                    var problem = new ProblemDetails
                    {
                        Title = "Validation Errors",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "One or more validation errors occurred",
                        Extensions = { { "Errors", errors } }
                    };

                    context.Result = new BadRequestObjectResult(problem);
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}