using System;
using System.Collections.Generic;
using System.Linq;
using API.Models;
using API.ResourceParameters;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("/api/authors")]
    public class AuthorController : ControllerBase
    {
        private readonly ICourseLibraryRepository _repository;
        private readonly IMapper _mapper;

        public AuthorController(ICourseLibraryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<AuthorDto>> Get([FromQuery] AuthorsResourceParameters authorsResourceParameters)
        {
            var authors = _mapper.Map<IEnumerable<AuthorDto>>(_repository.GetAuthors(authorsResourceParameters));
            return Ok(authors);
        }

        [HttpGet("{authorId}")]
        public ActionResult<AuthorDto> Get(Guid authorId)
        {
            var author = _repository.GetAuthor(authorId);
            if (author == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<AuthorDto>(author));
        }
    }
}