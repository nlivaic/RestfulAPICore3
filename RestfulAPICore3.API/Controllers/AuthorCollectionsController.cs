using Microsoft.AspNetCore.Mvc;
using API.Services;
using AutoMapper;
using System.Collections.Generic;
using API.Models;
using System;
using API.Helpers;
using API.Entities;
using System.Linq;
using API.BaseControllers;
using Microsoft.AspNetCore.Http;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorCollectionsController : ApiControllerBase
    {
        private readonly ICourseLibraryRepository _repository;
        private readonly IMapper _mapper;

        public AuthorCollectionsController(ICourseLibraryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<AuthorDto>> Post(IEnumerable<AuthorForCreationDto> authors)
        {
            var newAuthors = _mapper.Map<IEnumerable<Author>>(authors);
            foreach (var newAuthor in newAuthors)
            {
                _repository.AddAuthor(newAuthor);
            }
            _repository.Save();
            var newAuthorIds = string.Join(',', newAuthors.Select(a => a.Id.ToString()).ToArray());
            var authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(newAuthors);
            return CreatedAtRoute("GetAuthorCollections", new { authorIds = newAuthorIds }, authorsToReturn);
        }

        [HttpGet("{authorIds}", Name = "GetAuthorCollections")]
        public ActionResult<IEnumerable<AuthorDto>> Get(
            [FromRoute]
            [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> authorIds)
        {
            var authors = _mapper.Map<IEnumerable<AuthorDto>>(_repository.GetAuthors(authorIds));
            return Ok(authors);
        }
    }
}