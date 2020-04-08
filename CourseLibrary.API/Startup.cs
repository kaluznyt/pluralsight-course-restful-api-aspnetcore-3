using AutoMapper;
using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using System;

namespace CourseLibrary.API
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddControllers(setupAction =>
      {
        setupAction.ReturnHttpNotAcceptable = true;
      })
      .AddNewtonsoftJson(setupAction =>
      {
        setupAction.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
      })
      .AddXmlDataContractSerializerFormatters()
      .ConfigureApiBehaviorOptions(ConfigureApiBehaviorOptions);

      services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

      services.AddScoped<ICourseLibraryRepository, CourseLibraryRepository>();

      services.AddDbContext<CourseLibraryContext>(options =>
            {
              options.UseSqlServer(
                  @"Server=(localdb)\mssqllocaldb;Database=CourseLibraryDB;Trusted_Connection=True;");
            });
    }

    private static void ConfigureApiBehaviorOptions(ApiBehaviorOptions setupAction)
    {
      setupAction.InvalidModelStateResponseFactory = context =>
      {
        var problemDetailsFactory = context.HttpContext.RequestServices
        .GetRequiredService<ProblemDetailsFactory>();

        var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext, context.ModelState);

        problemDetails.Detail = "See the errors field for details.";
        problemDetails.Instance = context.HttpContext.Request.Path;

        var actionExecutingContext = context as ActionExecutingContext;

        if (context.ModelState.ErrorCount > 0 //&&
       //actionExecutingContext?.ActionArguments.Count == context.ActionDescriptor.Parameters.Count
       )
        {
          problemDetails.Type = "https://courselibrary.com/modelvalidationproblem";
          problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
          problemDetails.Title = "One or more validation error occurred.";

          return new UnprocessableEntityObjectResult(problemDetails)
          {
            ContentTypes = { "application/problem+json" }
          };
        }


        problemDetails.Status = StatusCodes.Status400BadRequest;
        problemDetails.Title = "One or more errors on input occurred";
        return new BadRequestObjectResult(problemDetails)
        {
          ContentTypes = { "application/problem+json" }
        };

      };
    }



    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler(builder =>
        {
          builder.Run(async context =>
          {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("An unexpected fault happened. Try again");
          });
        });
      }

      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }
  }
}
