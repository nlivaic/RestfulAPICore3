using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using API.BaseControllers;
using API.Entities;
using API.Exceptions;
using API.Helpers;
using API.Models;
using API.ResourceParameters;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using RestfulAPICore3.API.Constraints;

namespace API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AuthorsController : ApiControllerBase
    {
        private readonly ICourseLibraryRepository _repository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IDataShapingService _dataShapingService;

        public AuthorsController(ICourseLibraryRepository repository, IMapper mapper, IPropertyMappingService propertyMappingService, IDataShapingService dataShapingService)
        {
            _repository = repository;
            _mapper = mapper;
            _propertyMappingService = propertyMappingService;
            _dataShapingService = dataShapingService;
        }

        /// <summary>
        /// Fetch a list of authors.
        /// </summary>
        /// <param name="authorsResourceParameters">Paging, filtering, searching, data shaping.</param>
        /// <returns>List of authors.</returns>
        /// <response code="200">Returns a list of authors.</response>
        [HttpGet(Name = "GetAuthors")]
        [HttpHead]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AuthorDto>))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult Get([FromQuery] AuthorsResourceParameters authorsResourceParameters)
        {
            PagedList<Author> pagedAuthors = null;
            try
            {
                pagedAuthors = _repository.GetAuthors(authorsResourceParameters);
            }
            catch (InvalidPropertyMappingException ex)
            {
                ModelState.AddModelError(nameof(AuthorsResourceParameters.OrderBy), ex.Message);
                return UnprocessableEntity();
            }
            var authorsDto = _mapper.Map<IEnumerable<AuthorDto>>(pagedAuthors);
            var paging = CreatePagingDto(pagedAuthors, authorsResourceParameters);
            Response.Headers.Add("X-Pagination", new StringValues(JsonSerializer.Serialize(paging)));
            // Data shaping service handles the case when fields are empty, resulting in all fields being sent to the client.
            // This is an expensive operation which in essence doesn't do anything since we already have the authors DTO,
            // so we are circumventing it since we know up front when no fields have been sent to us.
            IEnumerable<ExpandoObject> shapedAuthors = null;
            try
            {
                shapedAuthors = _dataShapingService.ShapeData(authorsDto, authorsResourceParameters.Fields);
            }
            catch (InvalidPropertyMappingException ex)
            {
                ModelState.AddModelError(nameof(AuthorsResourceParameters.Fields), ex.Message);
                return UnprocessableEntity();
            }
            var linkedAuthors = shapedAuthors.Select(author =>
            {
                IDictionary<string, object> authorAsDictionary = (IDictionary<string, object>)author;
                authorAsDictionary.Add("links", CreateAuthorLinks((Guid)authorAsDictionary["Id"], authorsResourceParameters.Fields));
                return authorAsDictionary as ExpandoObject;
            });
            var links = CreateAuthorsLinks(authorsResourceParameters, pagedAuthors.HasPreviousPage, pagedAuthors.HasNextPage);
            var authorsWithLinks = new
            {
                value = linkedAuthors,
                links
            };
            return Ok(authorsWithLinks);
        }

        /// <summary>
        /// Get author by id.
        /// </summary>
        /// <param name="authorId">Author's Id.</param>
        /// <param name="fields">A CSV list of fields on the resource the API should return.</param>
        /// <param name="acceptHeader">Allows client to 
        /// determine whether a shorter (friendly) version 
        /// of payload should be returned.
        /// HATEOAS links can also be returned.</param>
        /// <returns>Author.</returns>
        /// <response code="200">Returns the author based on id.</response>
        [HttpGet("{authorId}", Name = "GetAuthor")]
        [Produces("application/json",
            "application/xml",      // This is just for completeness because we also have the Xml formatter set up in Startup.
            "application/vnd.marvin.hateoas+json",
            "application/vnd.marvin.author.friendly+json",
            "application/vnd.marvin.author.friendly.hateoas+json",
            "application/vnd.marvin.author.full.hateoas+json",
            "application/vnd.marvin.author.full+json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthorDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public ActionResult Get(Guid authorId,
            [ModelBinder(typeof(ArrayModelBinder))] IEnumerable<string> fields,
            [FromHeader(Name = "Accept")]string acceptHeader
            )
        {
            if (!MediaTypeHeaderValue.TryParse(acceptHeader, out MediaTypeHeaderValue mediaType))
            {
                return BadRequest();
            }
            var author = _repository.GetAuthor(authorId);
            if (author == null)
            {
                return NotFound();
            }
            ExpandoObject shapedData = null;
            if (mediaType.Facets.Contains("friendly")
                || acceptHeader == "application/vnd.marvin.hateoas+json"
                || acceptHeader == "application/json")
            {
                var authorDto = _mapper.Map<AuthorDto>(author);
                try
                {
                    shapedData = _dataShapingService.ShapeData(authorDto, fields);
                }
                catch (InvalidPropertyMappingException ex)
                {
                    ModelState.AddModelError(nameof(AuthorsResourceParameters.Fields), ex.Message);
                    return UnprocessableEntity();
                }
            }
            else if (mediaType.Facets.Contains("full"))
            {
                var authorFullDto = _mapper.Map<AuthorFullDto>(author);
                try
                {
                    shapedData = _dataShapingService.ShapeData(authorFullDto, fields);
                }
                catch (InvalidPropertyMappingException ex)
                {
                    ModelState.AddModelError(nameof(AuthorsResourceParameters.Fields), ex.Message);
                    return UnprocessableEntity();
                }
            }
            if (mediaType.Facets.Contains("hateoas"))
            {
                var links = CreateAuthorLinks(authorId, fields);
                ((IDictionary<string, object>)shapedData).Add("links", links);
            }
            return Ok(shapedData);
        }

        [HttpPost(Name = "PostAuthor")]
        [Consumes("application/json",
            "application/vnd.marvin.authorforcreation+json")]
        [RequestHeaderMatchesMediaType("Content-Type",
            "application/json",
            "application/vnd.marvin.authorforcreation+json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public ActionResult<AuthorDto> Post(AuthorForCreationDto author)
        {
            var newAuthor = _mapper.Map<Author>(author);
            _repository.AddAuthor(newAuthor);
            _repository.Save();
            var authorDto = _mapper.Map<AuthorDto>(newAuthor);
            var shapedAuthor = _dataShapingService.ShapeData(authorDto) as IDictionary<string, object>;
            var links = CreateAuthorLinks(authorDto.Id, null);
            shapedAuthor.Add("links", links);
            return CreatedAtRoute("GetAuthor", new { authorId = authorDto.Id }, shapedAuthor);
        }

        // Note: this got commented out to accommodate Swashbuckle,
        //       since it has a problem distinguishing same
        //       (resource Uri, method) pairings according to Content-Type header.
        // [HttpPost(Name = "PostAuthorWithDateOfDeath")]
        // [Consumes("application/vnd.marvin.authorforcreationwithdateofdeath+json")]
        // [RequestHeaderMatchesMediaType("Content-Type",
        //     "application/vnd.marvin.authorforcreationwithdateofdeath+json")]
        // public ActionResult<AuthorDto> PostAuthorWithDateOfDeath(AuthorForCreationWithDateOfDeathDto author)
        // {
        //     var newAuthor = _mapper.Map<Author>(author);
        //     _repository.AddAuthor(newAuthor);
        //     _repository.Save();
        //     var authorDto = _mapper.Map<AuthorDto>(newAuthor);
        //     var shapedAuthor = _dataShapingService.ShapeData(authorDto) as IDictionary<string, object>;
        //     var links = CreateAuthorLinks(authorDto.Id, null);
        //     shapedAuthor.Add("links", links);
        //     return CreatedAtRoute("GetAuthor", new { authorId = authorDto.Id }, shapedAuthor);
        // }

        [HttpDelete("{authorId}", Name = "DeleteAuthor")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult Delete(Guid authorId)
        {
            var author = _repository.GetAuthor(authorId);
            if (author == null)
            {
                return NotFound();
            }
            _repository.DeleteAuthor(author);
            _repository.Save();
            return NoContent();
        }

        [HttpOptions]
        public IActionResult Options()
        {
            this.HttpContext.Response.Headers.Add("Allow", "POST,GET,HEAD");
            return Ok();
        }

        private PagingDto CreatePagingDto(PagedList<Author> pagedAuthors, AuthorsResourceParameters authorsResourceParameters)
        {
            var paging = new PagingDto(
                pagedAuthors.CurrentPage,
                pagedAuthors.TotalPages,
                pagedAuthors.TotalItems
            );
            return paging;
        }

        private string CreatePagingUri(AuthorsResourceParameters authorsResourceParameters, ResourceUriPagingType type)
        {
            return type switch
            {
                ResourceUriPagingType.Next
                    => this.Url.Link(
                        "GetAuthors",
                        new
                        {
                            authorsResourceParameters.Fields,
                            authorsResourceParameters.MainCategory,
                            authorsResourceParameters.SearchQuery,
                            authorsResourceParameters.OrderBy,
                            PageNumber = authorsResourceParameters.PageNumber + 1,
                            authorsResourceParameters.PageSize
                        }),
                ResourceUriPagingType.Prev
                    => this.Url.Link(
                        "GetAuthors",
                        new
                        {
                            authorsResourceParameters.Fields,
                            authorsResourceParameters.MainCategory,
                            authorsResourceParameters.SearchQuery,
                            authorsResourceParameters.OrderBy,
                            PageNumber = authorsResourceParameters.PageNumber - 1,
                            authorsResourceParameters.PageSize
                        }),
                ResourceUriPagingType.Current
                    => this.Url.Link(
                        "GetAuthors",
                        new
                        {
                            authorsResourceParameters.Fields,
                            authorsResourceParameters.MainCategory,
                            authorsResourceParameters.SearchQuery,
                            authorsResourceParameters.OrderBy,
                            authorsResourceParameters.PageNumber,
                            authorsResourceParameters.PageSize
                        }),
                _ => string.Empty
            };
        }

        private IEnumerable<LinkDto> CreateAuthorLinks(Guid authorId, IEnumerable<string> fields)
        {
            var links = new List<LinkDto>();
            links.Add(
                new LinkDto(
                    Url.Link("GetAuthor", new { authorId, fields }),
                    "self",
                    "GET"));
            links.Add(
                new LinkDto(
                    Url.Link("DeleteAuthor", new { authorId }),
                    "delete-author",
                    "DELETE"));
            links.Add(
                new LinkDto(
                    Url.Link("GetCoursesForAuthor", new { authorId }),
                    "get-courses-for-author",
                    "GET"));
            links.Add(
                new LinkDto(
                    Url.Link("PostCourseForAuthor", new { authorId }),
                    "post-course-for-author",
                    "POST"));
            return links;
        }

        private IEnumerable<LinkDto> CreateAuthorsLinks(AuthorsResourceParameters authorsResourceParameters, bool hasPreviousPage, bool hasNextPage)
        {
            List<LinkDto> links = new List<LinkDto>();
            links.Add(
                new LinkDto(
                    CreatePagingUri(authorsResourceParameters, ResourceUriPagingType.Current),
                    "self",
                    "GET"));
            if (hasPreviousPage)
            {
                links.Add(
                    new LinkDto(
                        CreatePagingUri(authorsResourceParameters, ResourceUriPagingType.Prev),
                        "prev-page",
                        "GET"
                    ));
            }
            if (hasNextPage)
            {
                links.Add(
                    new LinkDto(
                        CreatePagingUri(authorsResourceParameters, ResourceUriPagingType.Next),
                        "next-page",
                        "GET"
                    ));
            }
            return links;
        }
    }
}