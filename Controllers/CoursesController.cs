using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.Services.Applications;
using MyCourse.Models.ViewModels;
using System.Collections.Generic;
namespace MyCourse.Controllers;

public class CoursesController : Controller
{
    public IActionResult Index()
    {
        /*ASP.NET Core andrà a cercare una View che segue
         le convenzioni.*/
        CourseService courseService = new CourseService();
        List<CourseViewModel> courses = courseService.GetCourses();
        return View(courses);
    }
    public IActionResult Detail(string id)
    {
        return View();
    }
}