using AutoMapper;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseLibrary.API.Controllers
{
  [ApiController]
  [Route("api/authorcollections")]
  public class AuthorCollectionController : ControllerBase
  {
    private readonly ICourseLibraryRepository courseLibraryRepository;
    private readonly IMapper mapper;

    public AuthorCollectionController(
      ICourseLibraryRepository courseLibraryRepository,
      IMapper mapper)
    {
      this.courseLibraryRepository = courseLibraryRepository;
      this.mapper = mapper;
    }

    [HttpGet("({ids})", Name = "GetAuthorCollection")]
    public IActionResult GetAuthorCollection(
      [FromRoute]
      [ModelBinder(BinderType=typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
    {
      if (ids == null)
      {
        return BadRequest();
      }

      var authorEntities = courseLibraryRepository.GetAuthors(ids);

      if (ids.Count() != authorEntities.Count())
      {
        return NotFound();
      }

      return Ok(mapper.Map<IEnumerable<AuthorDto>>(authorEntities));
    }

    public ActionResult<IEnumerable<AuthorDto>> CreateAuthorCollection(
      IEnumerable<AuthorForCreationDto> authorCollection)
    {
      var authorEntities = mapper.Map<IEnumerable<Entities.Author>>(authorCollection);

      foreach (var author in authorEntities)
      {
        courseLibraryRepository.AddAuthor(author);
      }

      courseLibraryRepository.Save();

      var authorCollectionToReturn = mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
      var authorCollectionIds = string.Join(",", authorCollectionToReturn.Select(a => a.Id));

      return CreatedAtRoute("GetAuthorCollection", new { ids = authorCollectionIds }, authorCollectionToReturn);
    }


  }
}
