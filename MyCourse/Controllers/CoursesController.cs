using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.InputModels;
using MyCourse.Models.Services.Application;
using MyCourse.Models.ViewModels;

namespace MyCourse.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ICachedCourseService courseService;
        public CoursesController(ICachedCourseService courseService)
        {
            this.courseService = courseService;
        }
        public async Task<IActionResult> Index(CourseListInputModel model)
        {
            ViewData["Title"] = "Catalogo dei corsi";
            List<CourseViewModel> courses = await courseService.GetCoursesAsync(model);
            CourseListViewModel viewModel = new CourseListViewModel{
               Courses = courses,
               Input = model
            };
            return View(viewModel);
        }
        public async Task<IActionResult> Detail(int id)
        {
            CourseDetailViewModel course = await courseService.GetCourseAsync(id);
            ViewData["Title"] = course.Title;
            return View(course);
        }
    }
}