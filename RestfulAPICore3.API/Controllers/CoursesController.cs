using API.BaseControllers;
using API.Entities;
using API.Helpers;
using API.Models;
using API.Services;
using AutoMapper;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace API.Controllers
{
    [ApiController]
    [Route("/api/authors/{authorId}/[controller]")]
    // [ResponseCache(CacheProfileName = "240SecondsCacheProfile")]
    public class CoursesController : ApiControllerBase
    {
        private readonly ICourseLibraryRepository _repository;
        private readonly IMapper _mapper;

        public CoursesController(ICourseLibraryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet(Name = "GetCoursesForAuthor")]
        // [ResponseCache(Duration = 120)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = 60)]
        [HttpCacheValidation(MustRevalidate = false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        [HttpPost(Name = "PostCourseForAuthor")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        [HttpPut("{courseId}", Name = "PutCourse")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
                _repository.Save();
                var courseDto = _mapper.Map<CourseDto>(newCourse);
                return CreatedAtRoute("GetCourse", new { authorId, courseId }, courseDto);
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
                _repository.Save();
                return NoContent();
            }
        }

        /// <summary>
        /// Update a single property on Course.
        /// </summary>
        /// <param name="authorId">Author of the course.</param>
        /// <param name="courseId">Course you are updating.</param>
        /// <param name="course">Course Json Patch Document</param>
        /// <remarks>Course Json Patch Document Example: \
        /// PATCH /authors/authorid/courses/courseid \
        /// [ \
        ///     { \
        ///          "op": "replace", \
        ///          "path": "/title", \
        ///          "value": "New Title", \
        ///     } 
        /// ]
        /// </remarks>
        /// <returns></returns>
        [HttpPatch("{courseId}", Name = "PatchCourse")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult Patch(Guid authorId, Guid courseId, [FromBody] JsonPatchDocument<CourseForUpdateDto> course)
        {
            if (!_repository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var courseFromRepo = _repository.GetCourse(authorId, courseId);
            // Insert
            if (courseFromRepo == null)
            {
                var courseForUpdateDto = new CourseForUpdateDto();
                course.ApplyTo(courseForUpdateDto, ModelState);
                if (!TryValidateModel(courseForUpdateDto))
                {
                    return UnprocessableEntity();
                }
                var courseToAdd = _mapper.Map<Course>(courseForUpdateDto);
                courseToAdd.Id = courseId;
                _repository.AddCourse(authorId, courseToAdd);
                _repository.Save();
                return CreatedAtRoute("GetCourse", new { authorId, courseId }, courseToAdd);
            }
            else    // Update
            {
                var courseForUpdateDto = _mapper.Map<CourseForUpdateDto>(courseFromRepo);
                course.ApplyTo(courseForUpdateDto, ModelState);
                if (!TryValidateModel(courseForUpdateDto))
                {
                    return UnprocessableEntity();
                }
                _mapper.Map(courseForUpdateDto, courseFromRepo);
                _repository.UpdateCourse(courseFromRepo);
                _repository.Save();
                return NoContent();
            }
        }

        [HttpDelete("{courseId}", Name = "DeleteCourse")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(Guid authorId, Guid courseId)
        {
            var course = _repository.GetCourse(authorId, courseId);
            if (course == null)
            {
                return NotFound();
            }
            _repository.DeleteCourse(course);
            _repository.Save();
            return NoContent();
        }

        /// <summary>
        /// This method (as opposed to its Author counterpart) lacks `field` parameter because we have not implemented 
        /// data shaping for this resource, to keep things concise.
        /// </summary>
        private IEnumerable<LinkDto> CreateCourseLinks(Guid authorId, Guid? courseId = null)
        {
            List<LinkDto> links = new List<LinkDto>();
            links.Add(
                new LinkDto(
                    Url.Link("GetCourse", new { authorId, courseId.Value }),
                    "self",
                    "GET"
                )
            );
            links.Add(
                new LinkDto(
                    Url.Link("GetCoursesForAuthor", new { authorId }),
                    "get-courses",
                    "GET"
                )
            );
            links.Add(
                new LinkDto(
                    Url.Link("PutCourse", new { authorId, courseId.Value }),
                    "put-course",
                    "PUT"
                )
            );
            links.Add(
                new LinkDto(
                    Url.Link("PatchCourse", new { authorId, courseId.Value }),
                    "patch-course",
                    "PATCH"
                )
            );
            links.Add(
                new LinkDto(
                    Url.Link("DeleteCourse", new { authorId, courseId.Value }),
                    "delete-course",
                    "DELETE"
                )
            );
            return links;
        }

        private IEnumerable<LinkDto> CreateCoursesLinks(Guid authorId)
        {
            List<LinkDto> links = new List<LinkDto>();
            links.Add(
                new LinkDto(
                    Url.Link("GetCourses", new { authorId }),
                    "self",
                    "GET"
                )
            );
            return links;
        }
    }
}