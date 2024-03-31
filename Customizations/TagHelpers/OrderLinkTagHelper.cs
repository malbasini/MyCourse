using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MyCourse.Models.InputModels;

namespace MyCourse.Customizations.TagHelpers
{
    /*--
       Nel codice che hai fornito, "OrderLinkTagHelper" è una classe Tag Helper 
       personalizzata che estende la classe AnchorTagHelper. Gli helper tag in 
       ASP.NET Core sono una nuova funzionalità che consente al codice 
       lato server di partecipare alla creazione e al rendering di elementi 
       HTML nei file Razor.
       
       La classe `OrderLinkTagHelper` viene utilizzata qui per generare tag 
       di ancoraggio HTML ("a") personalizzati. Le proprietà "OrderBy" e "Input" 
       vengono utilizzate per recuperare i dati richiesti per il collegamento.
       
       Il "generatore IHtmlGenerator" viene inserito nel costruttore chiamato dalla 
       classe base "AnchorTagHelper".
       
       "AnchorTagHelper" genera tag di ancoraggio HTML e la classe "OrderLinkTagHelper" 
       sovrascrive il metodo "Process" in cui puoi controllare come viene generato il 
       tag di ancoraggio HTML.
       
       Ecco una breve panoramica basata sul codice fornito:
       
       - Il metodo `Process()` aggiorna la proprietà `RouteValues[]` sulla classe 
       `AnchorTagHelper` con i valori per `search`, `orderby` e `ascending` in base 
       alle proprietà di `OrderLinkTagHelper` e `CourseListInputModel `
       
       - Viene chiamato `base.Process(context, output);` per creare il tag `a` 
       con i valori della route aggiornati.
       
       - Se la proprietà "OrderBy" sull'oggetto "Input" è uguale alla proprietà "OrderBy" su "OrderLinkTagHelper", aggiunge un'icona Font-Awesome per indicare la direzione di ordinamento utilizzando "output.PostContent.SetHtmlContent()".
       
       La classe "CourseListInputModel" è un modello di input utilizzato per mantenere 
       lo stato di come l'utente desidera elencare i corsi. Viene utilizzato come 
       proprietà nella classe "OrderLinkTagHelper".
       
       Allo stesso modo, la classe "CourseListInputModelBinder" è un raccoglitore 
       di modelli personalizzato che associa una richiesta HTTP a un metodo di azione, 
       come spiegato in "CourseListInputModelBinder.cs".
       
       Infine, le classi "CoursesOptions" e "CoursesOrderOptions" da "CoursesOptions.cs" 
       vengono utilizzate principalmente per contenere le impostazioni degli attributi 
       comuni di Courses.
       
       ==> Nel complesso, questo codice è un ottimo esempio di come è possibile 
       estendere gli helper tag esistenti in ASP.NET Core per eseguire la 
       generazione di codice HTML personalizzato in base ai requisiti dell'applicazione.
     */
    public class OrderLinkTagHelper : AnchorTagHelper
    {
        public string OrderBy { get; set; }
        public CourseListInputModel Input { get; set; }

        public OrderLinkTagHelper(IHtmlGenerator generator) : base(generator)
        {
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "a";

            //Imposto i valori del link
            RouteValues["search"] = Input.Search;
            RouteValues["orderby"] = OrderBy;
            RouteValues["ascending"] = (Input.OrderBy == OrderBy ? !Input.Ascending : Input.Ascending).ToString().ToLowerInvariant();
            
            //Faccio generare l'output all'AnchorTagHelper
            base.Process(context, output);

            //Aggiungo l'indicatore di direzione
            if (Input.OrderBy == OrderBy)
            {
                var direction = Input.Ascending ? "up" : "down";
                output.PostContent.SetHtmlContent($" <i class=\"fas fa-caret-{direction}\"></i>");
            }
        }
    }
}