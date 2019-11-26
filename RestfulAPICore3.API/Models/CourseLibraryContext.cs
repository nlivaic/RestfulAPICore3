using Microsoft.EntityFrameworkCore;
using API.Entities;

namespace API.Models
{
    public class CourseLibraryContext : DbContext
    {
        public DbSet<Author> Authors { get; set; }
        public DbSet<Course> Courses { get; set; }

        public CourseLibraryContext(DbContextOptions options) : base(options)
        {
        }
    }
}