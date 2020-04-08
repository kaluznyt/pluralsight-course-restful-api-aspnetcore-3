using AutoMapper;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace CourseLibrary.API.Controllers
{
  [ApiController]
  [Route("api/authors")]
  public class AuthorsController : ControllerBase
  {
    private readonly ICourseLibraryRepository courseLibraryRepository;
    private readonly IMapper mapper;

    public AuthorsController(
      ICourseLibraryRepository courseLibraryRepository,
      IMapper mapper)
    {
      this.courseLibraryRepository = courseLibraryRepository;
      this.mapper = mapper;
    }

    [HttpGet]
    [HttpHead]
    public ActionResult<IEnumerable<AuthorDto>> GetAuthors(
      [FromQuery]AuthorsResourceParameters parameters)
    {
      return Ok(mapper.Map<IEnumerable<AuthorDto>>(courseLibraryRepository.GetAuthors(parameters)));
    }

    [HttpGet("{authorId:guid}", Name = "GetAuthor")]
    public IActionResult GetAuthor(Guid authorId)
    {
      var author = courseLibraryRepository.GetAuthor(authorId);

      return author != null ? Ok(mapper.Map<AuthorDto>(author)) : (IActionResult)NotFound();
    }

    [HttpPost]
    public ActionResult<AuthorDto> CreateAuthor(AuthorForCreationDto author)
    {
      var authorEntity = mapper.Map<Entities.Author>(author);
      courseLibraryRepository.AddAuthor(authorEntity);
      courseLibraryRepository.Save();
      var authorToReturn = mapper.Map<AuthorDto>(authorEntity);
      return CreatedAtRoute("GetAuthor", new { authorId = authorToReturn.Id }, authorToReturn);
    }

    [HttpDelete("{authorId}")]
    public ActionResult DeleteAuthor(Guid authorId)
    {
      var author = courseLibraryRepository.GetAuthor(authorId);

      if (author == null)
      {
        return NotFound();
      }

      courseLibraryRepository.DeleteAuthor(author);
      courseLibraryRepository.Save();

      return NoContent();

    }

    [HttpOptions]
    public IActionResult GetAuthorOptions()
    {
      Response.Headers.Add("Allow", "GET,OPTIONS,POST");
      return Ok();
    }
  }
}