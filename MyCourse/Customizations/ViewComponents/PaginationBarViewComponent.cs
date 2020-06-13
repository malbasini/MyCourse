using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.ViewModels;

namespace MyCourse.Customizations.ViewComponents
{
    public class PaginationBarViewComponent : ViewComponent
    {
        //Ci serve il numero di pagina corrente
        //Il numero totale dei risultati
        //Il numero di risultati per pagina
        //Search OrderBy e Ascending
        public IViewComponentResult Invoke(IPaginationInfo model)
        {
            return View(model);
        }
    }
}