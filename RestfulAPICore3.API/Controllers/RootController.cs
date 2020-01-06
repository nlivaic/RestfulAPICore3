using System.Collections.Generic;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api")]
    public class RootController : ControllerBase
    {
        [HttpGet(Name = "Root")]
        public ActionResult<IEnumerable<LinkDto>> Get()
        {
            List<LinkDto> links = new List<LinkDto>();
            links.Add(
                new LinkDto(
                    Url.Link("Root", null),
                    "self",
                    "GET"
                )
            );
            links.Add(
                new LinkDto(
                    Url.Link("GetAuthors", null),
                    "get-authors",
                    "GET"
                )
            );
            links.Add(
                new LinkDto(
                    Url.Link("PostAuthor", null),
                    "post-author",
                    "POST"
                )
            );
            return links;
        }
    }
}