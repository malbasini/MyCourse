using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.Services.Applications;
using MyCourse.Models.ViewModels;
using System.Collections.Generic;
namespace MyCourse.Controllers;

public class CoursesController : Controller
{
    private readonly ICourseService courseService;

    public CoursesController(ICourseService courseService)
    {
        this.courseService = courseService;
    }
    public IActionResult Index()
    {
        /*ASP.NET Core andrà a cercare una View che segue
         le convenzioni.*/
        List<CourseViewModel> courses = courseService.GetCourses();
        return View(courses);
    }
    public IActionResult Detail(int id)
    {
        /*ASP.NET Core andrà a cercare una View che segue
         le convenzioni.*/
        CourseDetailViewModel viewModel = courseService.GetCourse(id);
        ViewData["Title"] = viewModel.Title;
        return View(viewModel);
    }
}