using CourseLibrary.API.Models;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.ValidationAttributes
{
  public class CourseTitleMustBeDifferentFromDescriptionAttribute : ValidationAttribute
  {
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
      if (validationContext.ObjectInstance is CourseForManipulationDto course && course.Title == course.Description)
      {
        return new ValidationResult(ErrorMessage, new[] { course.GetType().Name });
      }

      return ValidationResult.Success;

    }
  }
}
