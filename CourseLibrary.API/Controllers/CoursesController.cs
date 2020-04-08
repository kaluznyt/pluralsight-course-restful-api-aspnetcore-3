using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace CourseLibrary.API.Controllers
{
  [ApiController]
  [Route("api/authors/{authorId}/courses")]
  public class CoursesController : ControllerBase
  {
    private readonly ICourseLibraryRepository courseLibraryRepository;
    private readonly IMapper mapper;

    public CoursesController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper)
    {
      this.courseLibraryRepository = courseLibraryRepository;
      this.mapper = mapper;
    }

    [HttpGet()]
    public ActionResult<IEnumerable<CourseDto>> GetCoursesForAuthor(Guid authorId)
    {
      if (!courseLibraryRepository.AuthorExists(authorId))
      {
        return NotFound();
      }

      var courses = courseLibraryRepository.GetCourses(authorId);
      return Ok(mapper.Map<IEnumerable<CourseDto>>(courses));
    }

    [HttpGet("{courseId}", Name = "GetCourseForAuthor")]
    public ActionResult<IEnumerable<CourseDto>> GetCourseForAuthor(Guid authorId, Guid courseId)
    {
      if (!courseLibraryRepository.AuthorExists(authorId))
      {
        return NotFound();
      }

      var course = courseLibraryRepository.GetCourse(authorId, courseId);

      if (course == null)
      {
        return NotFound();
      }

      return Ok(mapper.Map<CourseDto>(course));
    }

    [HttpPost]
    public ActionResult<CourseDto> CreateCourseForAuthor(Guid authorId, CourseForCreationDto course)
    {
      if (!courseLibraryRepository.AuthorExists(authorId))
      {
        return NotFound();
      }

      var courseEntity = mapper.Map<Entities.Course>(course);
      courseLibraryRepository.AddCourse(authorId, courseEntity);
      courseLibraryRepository.Save();

      var courseToReturn = mapper.Map<CourseDto>(courseEntity);

      return CreatedAtRoute("GetCourseForAuthor", new { authorId = courseToReturn.AuthorId, courseId = courseToReturn.Id }, courseToReturn);

    }

    [HttpPut("{courseId}")]
    public IActionResult UpdateCourseForAuthor(Guid authorId, Guid courseId, CourseForUpdateDto course)
    {
      if (!courseLibraryRepository.AuthorExists(authorId))
      {
        return NotFound();
      }

      var courseFromAuthorFromRepo = courseLibraryRepository.GetCourse(authorId, courseId);

      if (courseFromAuthorFromRepo == null)
      {
        var courseToAdd = mapper.Map<Course>(course);
        courseToAdd.Id = courseId;

        courseLibraryRepository.AddCourse(authorId, courseToAdd);
        courseLibraryRepository.Save();

        var courseToReturn = mapper.Map<CourseDto>(courseToAdd);

        return CreatedAtRoute("GetCourseForAuthor", new { authorId, courseId = courseToAdd.Id }, courseToReturn);

        // return NotFound();
      }

      mapper.Map(course, courseFromAuthorFromRepo);

      courseLibraryRepository.UpdateCourse(courseFromAuthorFromRepo);

      courseLibraryRepository.Save();

      return NoContent();

    }

    [HttpPatch("{courseId}")]
    public ActionResult PartiallyUpdateCourseForAuthor(Guid authorId, Guid courseId, JsonPatchDocument<CourseForUpdateDto> patchDocument)
    {
      if (!courseLibraryRepository.AuthorExists(authorId))
      {
        return NotFound();
      }

      var courseForAuthorFromRepo = courseLibraryRepository.GetCourse(authorId, courseId);

      if (courseForAuthorFromRepo == null)
      {
        var courseDto = new CourseForUpdateDto();
        patchDocument.ApplyTo(courseDto, ModelState);

        if (!TryValidateModel(courseDto))
        {
          return ValidationProblem(ModelState);
        }

        var courseToAdd = mapper.Map<Course>(courseDto);
        courseToAdd.Id = courseId;

        courseLibraryRepository.AddCourse(authorId, courseToAdd);
        courseLibraryRepository.Save();

        var courseToReturn = mapper.Map<CourseDto>(courseToAdd);

        return CreatedAtRoute("GetCourseForAuthor", new { authorId, courseId = courseToAdd.Id }, courseToReturn);
      }

      var courseToPatch = mapper.Map<CourseForUpdateDto>(courseForAuthorFromRepo);

      patchDocument.ApplyTo(courseToPatch, ModelState);

      if (!TryValidateModel(courseToPatch))
      {
        return ValidationProblem(ModelState);
      }

      mapper.Map(courseToPatch, courseForAuthorFromRepo);

      courseLibraryRepository.UpdateCourse(courseForAuthorFromRepo);

      courseLibraryRepository.Save();

      return NoContent();
    }

    [HttpDelete("{courseId}")]
    public ActionResult DeleteCourseForAuthor(Guid authorId, Guid courseId)
    {
      if (!courseLibraryRepository.AuthorExists(authorId))
      {
        return NotFound();
      }

      var courseToDelete = courseLibraryRepository.GetCourse(authorId, courseId);

      if (courseToDelete == null)
      {
        return NotFound();
      }

      courseLibraryRepository.DeleteCourse(courseToDelete);
      courseLibraryRepository.Save();

      return NoContent();
    }

    public override ActionResult ValidationProblem([ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
    {
      var options = HttpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>();

      return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
    }
  }
}
