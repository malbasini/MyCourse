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
            ManageError handler = new ManageError(HttpContext);
            handler.Manage();
            ViewBag.Title = handler.Title;
            Response.StatusCode = handler.StatusCode;
            return View(handler.ViewName);  
        }
    }
}