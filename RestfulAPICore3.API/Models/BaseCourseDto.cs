using System.ComponentModel.DataAnnotations;
using API.ValidationAttributes;

namespace API.Models
{
    [CourseDtoValidationAttribute(ErrorMessage = "Some custom error")]
    public abstract class BaseCourseDto
    {
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }
        [MaxLength(50)]
        public virtual string Description { get; set; }
    }
}