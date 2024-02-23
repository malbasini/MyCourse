using Microsoft.AspNetCore.Mvc;

namespace MyCourse.Controllers;

public class CoursesController : Controller
{
    public IActionResult Index()
    {
        return Content("Sono l'action Index");
    }
    public IActionResult Detail(string id)
    {
        return Content($"Sono Detail e ho ricevuto l'id {id}");
    }
}