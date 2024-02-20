using Microsoft.AspNetCore.Mvc;

namespace MyCourse.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return Content("Sono l'action Index del controller Home");
    }
}