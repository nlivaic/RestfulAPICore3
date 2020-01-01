using API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using API.Entities;
using API.ResourceParameters;
using API.Helpers;

namespace API.Services
{
    public class CourseLibraryRepository : ICourseLibraryRepository, IDisposable
    {
        private readonly CourseLibraryContext _context;
        private readonly IPagingService _pagingService;
        private readonly IPropertyMappingService _propertyMappingService;

        public CourseLibraryRepository(CourseLibraryContext context, IPagingService pagingService, IPropertyMappingService propertyMappingService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _pagingService = pagingService;
            _propertyMappingService = propertyMappingService;
        }

        public void AddCourse(Guid authorId, Course course)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(authorId));
            }

            if (course == null)
            {
                throw new ArgumentNullException(nameof(course));
            }
            // always set the AuthorId to the passed-in authorId
            course.AuthorId = authorId;
            _context.Courses.Add(course);
        }

        public void DeleteCourse(Course course)
        {
            _context.Courses.Remove(course);
        }

        public Course GetCourse(Guid authorId, Guid courseId)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(authorId));
            }

            if (courseId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(courseId));
            }

            return _context.Courses
              .Where(c => c.AuthorId == authorId && c.Id == courseId).FirstOrDefault();
        }

        public IEnumerable<Course> GetCourses(Guid authorId)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(authorId));
            }

            return _context.Courses
                        .Where(c => c.AuthorId == authorId)
                        .OrderBy(c => c.Title).ToList();
        }

        public void UpdateCourse(Course course)
        {
            // no code in this implementation
        }

        public void AddAuthor(Author author)
        {
            if (author == null)
            {
                throw new ArgumentNullException(nameof(author));
            }

            // the repository fills the id (instead of using identity columns)
            author.Id = Guid.NewGuid();

            foreach (var course in author.Courses)
            {
                course.Id = Guid.NewGuid();
            }

            _context.Authors.Add(author);
        }

        public bool AuthorExists(Guid authorId)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(authorId));
            }

            return _context.Authors.Any(a => a.Id == authorId);
        }

        public void DeleteAuthor(Author author)
        {
            if (author == null)
            {
                throw new ArgumentNullException(nameof(author));
            }

            _context.Authors.Remove(author);
        }

        public Author GetAuthor(Guid authorId)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(authorId));
            }

            return _context.Authors.FirstOrDefault(a => a.Id == authorId);
        }

        public IEnumerable<Author> GetAuthors()
        {
            return _context.Authors.ToList<Author>();
        }

        public PagedList<Author> GetAuthors(AuthorsResourceParameters authorsResourceParameters)
        {
            var query = _context.Authors as IQueryable<Author>;
            if (!string.IsNullOrEmpty(authorsResourceParameters.MainCategory))
            {
                query = query.Where(a => a.MainCategory == authorsResourceParameters.MainCategory);
            }
            if (!string.IsNullOrEmpty(authorsResourceParameters.SearchQuery))
            {
                query = query.Where(a =>
                    a.MainCategory.Contains(authorsResourceParameters.SearchQuery) ||
                    a.FirstName.Contains(authorsResourceParameters.SearchQuery) ||
                    a.LastName.Contains(authorsResourceParameters.SearchQuery));
            }
            if (!string.IsNullOrEmpty(authorsResourceParameters.OrderBy))
            {
                var orderByCriteria = authorsResourceParameters.OrderByWithDirection();
                var targetProperties = _propertyMappingService
                    .GetMappings<AuthorDto, Author>(
                        orderByCriteria.Select(o => o.Item1).ToArray());
                targetProperties.ToList()
                    .ForEach(
                        tp => tp.Revert = orderByCriteria
                            .Single(o => string.Equals(o.Item1, tp.SourcePropertyName, StringComparison.OrdinalIgnoreCase))
                            .Item2 == OrderingDirection.Asc
                                ? tp.Revert
                                : !tp.Revert
                    );
                query = query.ApplySort(targetProperties);
            }
            return _pagingService.PageList(query, authorsResourceParameters.PageNumber, authorsResourceParameters.PageSize);
        }

        public IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds)
        {
            if (authorIds == null)
            {
                throw new ArgumentNullException(nameof(authorIds));
            }

            return _context.Authors.Where(a => authorIds.Contains(a.Id))
                .OrderBy(a => a.FirstName)
                .OrderBy(a => a.LastName)
                .ToList();
        }

        public void UpdateAuthor(Author author)
        {
            // no code in this implementation
        }

        public bool Save()
        {
            return (_context.SaveChanges() >= 0);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose resources when needed
            }
        }
    }
}