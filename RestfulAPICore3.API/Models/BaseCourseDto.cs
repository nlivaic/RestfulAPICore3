using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public abstract class BaseCourseDto
    {
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }
        [MaxLength(50)]
        public virtual string Description { get; set; }
    }
}