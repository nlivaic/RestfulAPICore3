using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace API.Helpers
{
    public interface IValidationProblemDetailsFactory
    {
        ValidationProblemDetails Create(ActionContext context);
    }
    public class ValidationProblemDetailsFactory
    {
        public static ValidationProblemDetails Create(ActionContext actionContext)
        {
            var problemDetails = new ValidationProblemDetails(actionContext.ModelState)
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "One or more validation errors occurred.",
                Detail = "See the errors property for more details.",
                Instance = actionContext.HttpContext.Request.Path
            };
            problemDetails.Extensions.Add("traceId", actionContext.HttpContext.TraceIdentifier);
            return problemDetails;
        }
    }
}