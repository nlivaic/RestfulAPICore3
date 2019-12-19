using System;
using System.Collections.Generic;
using API.BaseControllers;
using API.Entities;
using API.Helpers;
using API.Models;
using API.ResourceParameters;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AuthorsController : ApiControllerBase
    {
        private readonly ICourseLibraryRepository _repository;
        private readonly IMapper _mapper;

        public AuthorsController(ICourseLibraryRepository repository, IMapper mapper, IInvalidModelResultFactory invalidModelResultFactory)
            : base(invalidModelResultFactory)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        [HttpHead]
        public ActionResult<IEnumerable<AuthorDto>> Get([FromQuery] AuthorsResourceParameters authorsResourceParameters)
        {
            var authors = _mapper.Map<IEnumerable<AuthorDto>>(_repository.GetAuthors(authorsResourceParameters));
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
    }
}