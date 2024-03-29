using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using MyCourse.Models.InputModels;
using MyCourse.Models.Options;

namespace MyCourse.Customizations.ModelBinders
{
    public class CourseListInputModelBinder : IModelBinder
    {
        private readonly IOptionsMonitor<CoursesOptions> coursesOptions;
        public CourseListInputModelBinder(IOptionsMonitor<CoursesOptions> coursesOptions)
        {
            this.coursesOptions = coursesOptions;
        }
        /*--
         Il metodo "BindModelAsync" è definito nella classe "CourseListInputModelBinder" che implementa 
         l'interfaccia "IModelBinder". Questa classe è uno strumento di associazione di modelli personalizzato, 
         utilizzato da ASP.NET Core per associare i dati della richiesta HTTP ai parametri delle Action.
         In questo metodo, `bindingContext.ValueProvider` fornisce valori dalla richiesta HTTP. 
         I dati della richiesta vengono recuperati e trasformati in tipologie adeguate. Quindi, 
         con questi dati viene creata una nuova istanza di `CourseListInputModel`. Questa nuova istanza viene 
         quindi impostata come risultato per il contesto di associazione per indicare che il modello è stato 
         associato correttamente. Infine, il metodo restituisce "Task.CompletedTask" perché è un metodo asincrono, 
         ma in questo caso non viene eseguito alcun lavoro asincrono effettivo.
         Infine, questo raccoglitore di modelli personalizzati viene utilizzato nella classe 
         "CourseListInputModel" utilizzando l'attributo "ModelBinder".
         Pertanto, quando ASP.NET Core rileva un parametro di tipo "CourseListInputModel" in una Action, 
         utilizzerà "CourseListInputModelBinder" per associare i dati della richiesta HTTP a tale parametro.
         */
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            //Recuperiamo i valori grazie ai value provider
            string search = bindingContext.ValueProvider.GetValue("Search").FirstValue;
            string orderBy = bindingContext.ValueProvider.GetValue("OrderBy").FirstValue;
            int.TryParse(bindingContext.ValueProvider.GetValue("Page").FirstValue, out int page);
            bool.TryParse(bindingContext.ValueProvider.GetValue("Ascending").FirstValue, out bool ascending);

            //Creiamo l'istanza del CourseListInputModel
            CoursesOptions options = coursesOptions.CurrentValue;
            var inputModel = new CourseListInputModel(search, page, orderBy, ascending, (int)options.PerPage, options.Order);

            //Impostiamo il risultato per notificare che la creazione è avvenuta con successo
            bindingContext.Result = ModelBindingResult.Success(inputModel);

            //Restituiamo un task completato
            return Task.CompletedTask;
        }
    }
}