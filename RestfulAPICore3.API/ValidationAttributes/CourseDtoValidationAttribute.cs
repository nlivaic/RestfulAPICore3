using System.ComponentModel.DataAnnotations;
using API.Models;

namespace API.ValidationAttributes
{
    public class CourseDtoValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            CourseForCreationDto course = (CourseForCreationDto)value;
            if (course.Title == course.Description)
            {
                return new ValidationResult($"{nameof(course.Title)} and {nameof(course.Description)} cannot be the same value.", new[] { nameof(CourseForCreationDto) });
            }
            return ValidationResult.Success;
        }
    }
}