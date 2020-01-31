using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.BaseControllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        private readonly IInvalidModelResultFactory _invalidModelResultFactory;

        public ApiControllerBase(IInvalidModelResultFactory invalidModelResultFactory)
        {
            _invalidModelResultFactory = invalidModelResultFactory;
        }

        protected new UnprocessableEntityObjectResult UnprocessableEntity()
            => (UnprocessableEntityObjectResult)_invalidModelResultFactory.Create(ControllerContext);
    }
}