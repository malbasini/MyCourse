using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.InputModels;
using MyCourse.Models.Services.Applications;
using MyCourse.Models.ViewModels;

namespace MyCourse.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ICourseService courseService;
        public CoursesController(ICachedCourseService courseService)
        {
            this.courseService = courseService;
        }
        public async Task<IActionResult> Index(CourseListInputModel input)
        {
            ViewData["Title"] = "Catalogo dei corsi";
            ListViewModel<CourseViewModel> courses = await courseService.GetCoursesAsync(input);
            
            CourseListViewModel viewModel = new CourseListViewModel
            {
                Courses = courses,
                Input = input
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Detail(int id)
        {
            CourseDetailViewModel viewModel = await courseService.GetCourseAsync(id);
            ViewData["Title"] = viewModel.Title;
            return View(viewModel);
        }
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Nuovo corso";
            var inputModel = new CourseCreateInputModel();
            return View(inputModel);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CourseCreateInputModel inputModel)
        {
            if (!ModelState.IsValid)
            {
                return View(inputModel);
            }
            CourseDetailViewModel course = await courseService.CreateCourseAsync(inputModel);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> IsTitleAvailable(string title, int id = 0)
        {
            bool result = await courseService.IsTitleAvailableAsync(title, id);
            return Json(result);
        }
    }
}