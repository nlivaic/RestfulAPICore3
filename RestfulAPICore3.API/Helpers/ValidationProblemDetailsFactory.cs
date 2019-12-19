using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace API.Helpers
{
    public interface IValidationProblemDetailsFactory
    {
        Func<ActionContext, int> InvalidModelStateHttpStatusCode { get; }
        ValidationProblemDetails Create(ActionContext context);
    }
    public class ValidationProblemDetailsFactory : IValidationProblemDetailsFactory
    {
        public Func<ActionContext, int> InvalidModelStateHttpStatusCode { get; } = context =>
            context.ModelState.Root.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid
                    ? StatusCodes.Status400BadRequest
                    : StatusCodes.Status422UnprocessableEntity;

        public ValidationProblemDetails Create(ActionContext context)
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "One or more validation errors occurred.",
                Status = InvalidModelStateHttpStatusCode(context),
                Detail = "See the errors property for more details.",
                Instance = context.HttpContext.Request.Path
            };
            problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);
            return problemDetails;
        }
    }
}