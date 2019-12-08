using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class CourseForUpdateDto : BaseCourseDto
    {
        [Required]
        public override string Description
        {
            get => base.Description;
            set
            {
                base.Description = value;
            }
        }
    }
}