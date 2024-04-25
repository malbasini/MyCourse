using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MyCourse.Customizations.TagHelpers
{
    /*--
           Il codice C# che hai condiviso fa parte di un "TagHelper" in ASP.NET Core. 
           Questo "TagHelper" concreto si chiama "RatingTagHelper". Gli helper tag in 
           ASP.NET Core rappresentano un modo per partecipare al rendering degli 
           elementi HTML nelle visualizzazioni Razor.
           
           La classe "TagHelper" che possiedi definisce una proprietà "Value" e 
           sovrascrive un metodo della classe base "TagHelper": "Process".
           
           La proprietà "Valore" indica il valore di valutazione con cui funzionerà 
           questo helper tag.
           
           Il metodo "Process" è il nucleo dell'helper tag, dove viene applicata 
           la logica di elaborazione. Questo metodo viene chiamato quando Razor 
           incontra l'helper tag in una vista. Fornisce un oggetto `TagHelperContext` 
           e un oggetto `TagHelperOutput` come parametri.
           
           L'oggetto "TagHelperContext", denominato "context" nel metodo, 
           contiene informazioni sulla visualizzazione Razor corrente di cui 
           viene eseguito il rendering, incluso il tag che ha attivato questo helper tag.
           
           L'oggetto "TagHelperOutput", denominato "output" nel metodo, è un oggetto 
           che l'helper tag può utilizzare per modificare il tag che viene scritto 
           nell'output.
           
           Nel tuo metodo "Process", utilizzi la proprietà "Value" per generare 
           output HTML variabili. Esegui il loop sui valori da 1 a 5 (inclusi). 
           Se il "Valore" è maggiore o uguale all'indice del loop, aggiungi 
           un codice HTML per una stella intera. Se il "Valore" è inferiore 
           ma maggiore dell'indice "i - 1", aggiungi un codice HTML per una 
           mezza stella e, infine, se il "Valore" è inferiore all'indice "i - 1", 
           aggiungi un codice HTML per una stella vuota. Questo HTML viene 
           aggiunto al contenuto di "output", alterando quindi l'aspetto dell'HTML 
           finale renderizzato.
         */
    public class RatingTagHelper : TagHelper
    {
        public double Value { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            //double value = (double) context.AllAttributes["value"].Value;

  		    for(int i = 1; i <= 5; i++)
            {
                if (Value >= i)
                {
                    output.Content.AppendHtml("<i class=\"fas fa-star\"></i>");
                }
                else if (Value > i - 1)
                {
                    output.Content.AppendHtml("<i class=\"fas fa-star-half-alt\"></i>");
                }
                else
                {
                    output.Content.AppendHtml("<i class=\"far fa-star\"></i>");
                }
            }          
		}
    }
}