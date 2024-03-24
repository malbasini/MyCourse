using Microsoft.AspNetCore.Mvc;

namespace MyCourse.Controllers
{
    public class HomeController : Controller
    {
        /*--ResponseCache mette in Cache la View dell'action Index per 60 secondi.
         Scaduti i 60 secondi il contenuto dovrà nuovamente essere preso dal server.*/
        //[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [ResponseCache(CacheProfileName = "Home")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Benvenuto su MyCourse!";
            return View();
        }
    }
}