using System;
using System.Collections.Generic;
using System.Linq;
using API.Entities;
using API.Models;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("/api/authors/{authorId}/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseLibraryRepository _repository;
        private readonly IMapper _mapper;

        public CoursesController(ICourseLibraryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CourseDto>> Get(Guid authorId)
        {
            if (!_repository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var courses = _repository.GetCourses(authorId);
            return Ok(_mapper.Map<IEnumerable<CourseDto>>(courses));
        }

        [HttpGet("{courseId}", Name = "GetCourse")]
        public ActionResult<CourseDto> Get(Guid authorId, Guid courseId)
        {
            if (!_repository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var course = _repository.GetCourse(authorId, courseId);
            if (course == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<CourseDto>(course));
        }

        [HttpPost]
        public ActionResult<CourseDto> Post(Guid authorId, CourseForCreationDto course)
        {
            if (!_repository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var newCourse = _mapper.Map<Course>(course);
            _repository.AddCourse(authorId, newCourse);
            _repository.Save();
            return CreatedAtRoute("GetCourse", new { authorId, courseId = newCourse.Id }, newCourse);
        }

        [HttpPut("{courseId}")]
        public ActionResult Put(Guid authorId, Guid courseId, CourseForUpdateDto course)
        {
            if (!_repository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var courseFromRepo = _repository.GetCourse(authorId, courseId);
            // Insert.
            if (courseFromRepo == null)
            {
                var newCourse = _mapper.Map<Course>(course);
                newCourse.Id = courseId;
                _repository.AddCourse(authorId, newCourse);
            }
            else    // Update.
            {
                // As per REST, we are updating the representation.
                // Therefore, we should:
                // 1. Get representation of entity.
                // 2. Map incoming Dto to that representation.
                // 3. Map updated representation to entity.
                // We can mimick those steps with below code directly:
                _mapper.Map(course, courseFromRepo);
                _repository.UpdateCourse(courseFromRepo);
            }
            _repository.Save();

            return NoContent();
        }
    }
}