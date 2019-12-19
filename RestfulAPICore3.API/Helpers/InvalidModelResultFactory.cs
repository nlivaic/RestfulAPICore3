using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Helpers
{
    public interface IInvalidModelResultFactory
    {
        IActionResult Create(ActionContext context);
    }

    public class InvalidModelResultFactory : IInvalidModelResultFactory
    {
        private readonly IValidationProblemDetailsFactory _validationProblemFactory;

        public InvalidModelResultFactory(IValidationProblemDetailsFactory validationProblemFactory)
        {
            _validationProblemFactory = validationProblemFactory;
        }

        public IActionResult Create(ActionContext context)
        {
            var validationProblem = _validationProblemFactory.Create(context);

            return validationProblem.Status switch
            {
                400 => new BadRequestObjectResult(validationProblem)
                {
                    ContentTypes = { "application/problem+json" }
                },
                422 => new UnprocessableEntityObjectResult(validationProblem)
                {
                    ContentTypes = { "application/problem+json" }
                },
                _ => throw new ArgumentException("Requested an unsupported invalid model status code. " +
                    $"Only {StatusCodes.Status400BadRequest} and {StatusCodes.Status422UnprocessableEntity} are supported.")
            };
        }
    }
}