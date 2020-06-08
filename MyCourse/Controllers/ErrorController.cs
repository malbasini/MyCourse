using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.Exceptions;
using MyCourse.Models.Services.Application;

namespace MyCourse.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            ManageError error = new ManageError(feature);
            error.Manage();
            ViewData["Title"] = error.Title;
            Response.StatusCode = error.StatusCode;
            return View(error.ViewName);  
        }
    }
}