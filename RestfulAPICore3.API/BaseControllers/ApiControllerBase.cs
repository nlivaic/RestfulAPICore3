using API.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.BaseControllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        protected new UnprocessableEntityObjectResult UnprocessableEntity()
        {
            var validationProblemDetails = ValidationProblemDetailsFactory.Create(ControllerContext);
            validationProblemDetails.Status = StatusCodes.Status422UnprocessableEntity;
            return UnprocessableEntity(validationProblemDetails);
        }
    }
}