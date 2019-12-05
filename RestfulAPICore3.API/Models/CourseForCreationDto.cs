using System.ComponentModel.DataAnnotations;
using API.ValidationAttributes;

namespace API.Models
{
    [CourseDtoValidationAttribute(ErrorMessage = "Some custom error")]
    public class CourseForCreationDto : BaseCourseDto
    {
        [Required]
        public override string Description { get { return base.Description; } set { base.Description = value; } }
    }
}