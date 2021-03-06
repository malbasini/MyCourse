using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.Exceptions;
using MyCourse.Models.InputModels;
using MyCourse.Models.Services.Application;
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

        public async Task<IActionResult> IsTitleAvailable(string title, int id=0)
        {
            bool result = await courseService.IsTitleAvailableAsync(title,id);
            return Json(result);
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
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CourseCreateInputModel inputModel)
        {
            if (ModelState.IsValid)
            try
            {
                 CourseDetailViewModel viewModel = await courseService.CreateCourseAsync(inputModel);
                 return RedirectToAction(nameof(Index));
            }
            catch(CourseTitleUnavailableException ex)
            {
                string message = ex.Message;
                ModelState.AddModelError(nameof(CourseDetailViewModel.Title), "Questo titolo esiste gi�");        
            }
            ViewData["Title"] = "Nuovo corso";
            return View(inputModel);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Modifica corso";
            CourseEditInputModel inputModel = await courseService.GetCourseForEditingAsync(id);
            return View(inputModel);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CourseEditInputModel inputModel)
        {
            if(ModelState.IsValid)
            {
                //Persisto i dati
                try
                {
                    CourseDetailViewModel course = await courseService.EditCourseAsync(inputModel);
                    TempData["ConfirmationMessage"] = "Aggiornamento eseguito correttamente";
                    return RedirectToAction(nameof(Detail), new {id= inputModel.Id});
                }
                catch(CourseTitleUnavailableException)
                {
                    ModelState.AddModelError(nameof(CourseDetailViewModel.Title), "Questo titolo esiste gi�");        
                }
            }
            ViewData["Title"] = "Modifica corso";
            return View(inputModel);
        }
    }
}