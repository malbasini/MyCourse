using Microsoft.AspNetCore.Mvc;

namespace MyCourse.Controllers;

public class CoursesController : Controller
{
    public IActionResult Index()
    {
        /*ASP.NET Core andrà a cercare una View che segue
         le convenzioni.*/
        return View();
    }
    public IActionResult Detail(string id)
    {
        return View();
    }
}