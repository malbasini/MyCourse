using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.Services.Application;
using MyCourse.Models.ViewModels;

namespace MyCourse.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ICourseService courseService;
        public CoursesController(ICourseService courseService)
        {
            this.courseService = courseService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Catalogo dei corsi";
            List<CourseViewModel> courses = courseService.GetCourses();
            return View(courses);
        }

        public IActionResult Detail(int id)
        {
            CourseDetailViewModel course = courseService.GetCourse(id);
            ViewData["Title"] = course.Title;
            return View(course);
        }
    }
}