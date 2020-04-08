using System;

namespace CourseLibrary.API.Helpers
{
  public static class DateTimeOffsetExtensions
  {
    public static int GetCurrentAge(this DateTimeOffset date)
    {
      return (DateTimeOffset.UtcNow.Year - date.Year);
    }
  }
}
