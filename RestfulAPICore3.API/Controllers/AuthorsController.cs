using System;
using System.Collections.Generic;
using System.Text.Json;
using API.BaseControllers;
using API.Entities;
using API.Exceptions;
using API.Helpers;
using API.Models;
using API.ResourceParameters;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AuthorsController : ApiControllerBase
    {
        private readonly ICourseLibraryRepository _repository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;

        public AuthorsController(ICourseLibraryRepository repository, IMapper mapper, IInvalidModelResultFactory invalidModelResultFactory, IPropertyMappingService propertyMappingService)
            : base(invalidModelResultFactory)
        {
            _repository = repository;
            _mapper = mapper;
            _propertyMappingService = propertyMappingService;
        }

        [HttpGet(Name = "GetAuthors")]
        [HttpHead]
        public ActionResult<IEnumerable<AuthorDto>> Get([FromQuery] AuthorsResourceParameters authorsResourceParameters)
        {
            PagedList<Author> pagedAuthors = null;
            try
            {
                pagedAuthors = _repository.GetAuthors(authorsResourceParameters);
            }
            catch (InvalidOrderByCriteriaException ex)
            {
                ModelState.AddModelError(nameof(AuthorsResourceParameters.OrderBy), ex.Message);
                return UnprocessableEntity();
            }
            var authors = _mapper.Map<IEnumerable<AuthorDto>>(pagedAuthors);
            var paging = CreatePagingDto(pagedAuthors, authorsResourceParameters);
            Response.Headers.Add("X-Pagination", new StringValues(JsonSerializer.Serialize(paging)));
            return Ok(authors);
        }

        [HttpGet("{authorId}", Name = "GetAuthor")]
        public ActionResult<AuthorDto> Get(Guid authorId)
        {
            var author = _repository.GetAuthor(authorId);
            if (author == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<AuthorDto>(author));
        }

        [HttpPost]
        public ActionResult<AuthorDto> Post(AuthorForCreationDto author)
        {
            var newAuthor = _mapper.Map<Author>(author);
            var newCourses = _mapper.Map<IEnumerable<Course>>(author.Courses);
            _repository.AddAuthor(newAuthor);
            _repository.Save();
            var authorDto = _mapper.Map<AuthorDto>(newAuthor);
            return CreatedAtRoute("GetAuthor", new { authorId = authorDto.Id }, authorDto);
        }

        [HttpDelete("{authorId}")]
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
                pagedAuthors.TotalItems,
                pagedAuthors.HasPreviousPage
                    ? this.Url.Link(
                        "GetAuthors",
                        new
                        {
                            authorsResourceParameters.MainCategory,
                            authorsResourceParameters.SearchQuery,
                            authorsResourceParameters.OrderBy,
                            PageNumber = pagedAuthors.CurrentPage - 1,
                            authorsResourceParameters.PageSize
                        })
                    : null,
                pagedAuthors.HasNextPage
                    ? this.Url.Link(
                        "GetAuthors",
                        new
                        {
                            authorsResourceParameters.MainCategory,
                            authorsResourceParameters.SearchQuery,
                            authorsResourceParameters.OrderBy,
                            PageNumber = pagedAuthors.CurrentPage + 1,
                            authorsResourceParameters.PageSize
                        })
                    : null
                );
            return paging;
        }
    }
}