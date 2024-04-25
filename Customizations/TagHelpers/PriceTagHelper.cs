using System.Text;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MyCourse.Models.ValueObjects;

namespace MyCourse.Customizations.TagHelpers
{
    /*--
     Il `PriceTagHelper` è un [Tag Helper]
     (https://docs.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/intro?view=aspnetcore-6.0) in ASP.NET Core . Gli helper tag consentono al codice lato server di partecipare alla creazione e al rendering di elementi HTML nei file Razor.
       
       "PriceTagHelper" eredita da "TagHelper". Ha due proprietà **`CurrentPrice`** 
       e **`FullPrice`** di tipo "Money". "Money" è un oggetto di valore che 
       rappresenta un valore monetario. Include l'importo e il tipo di valuta 
       (come EUR, USD, GPB ecc.).
       
       Il metodo "Process" in "PriceTagHelper" sovrascrive il metodo "Process" 
       della classe base. Nel metodo "Process", assegna il nome del tag come "span" 
       e quindi aggiunge "CurrentPrice" al contenuto di output.
       
       Se "CurrentPrice" e "FullPrice" non sono uguali, anche "FullPrice" 
       viene aggiunto all'output ma si trova tra i tag "<s>", cancellandolo 
       di fatto e suggerendo un prezzo scontato.
       
       Ecco lo snippet di codice annotato che riassume ciò che fa questa classe:
       
       // Eredita da TagHelper per ridefinire il rendering di uno specifico elemento HTML
       classe pubblica PriceTagHelper: TagHelper
       {
           // proprietà per contenere i valori del prezzo corrente e completo
           public Money PrezzoCorrente { get; impostato; }
           public Money Prezzo intero { get; impostato; }
       
           // Questo metodo imposta la logica di rendering per l'helper tag
           // Aggiunge i valori del prezzo al tag.
           public override void Process (contesto TagHelperContext, output TagHelperOutput)
           {
               // Il tag nella vista a cui viene applicato verrà sostituito da un <span>
               output.TagName = "intervallo";
               // Il prezzo corrente viene aggiunto all'interno dell'intervallo
               output.Content.AppendHtml($"{CurrentPrice}");
       
               // Se c'è una discrepanza tra il prezzo intero e il prezzo corrente, il prezzo intero
               // viene visualizzato come testo barrato sotto il prezzo corrente che suggerisce uno sconto
               if(!Prezzoattuale.Equals(Prezzointero)) {
                   output.Content.AppendHtml($"<br><s>{FullPrice}</s>");
               }
           }
       }
       
       
       Ogni volta che questo helper tag viene richiamato nella vista, genera un 
       tag "span" con i prezzi di conseguenza.
     */
    public class PriceTagHelper : TagHelper
    {
        public Money CurrentPrice { get; set; }
        public Money FullPrice { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            output.Content.AppendHtml($"{CurrentPrice}");

            if(!CurrentPrice.Equals(FullPrice)) {
                output.Content.AppendHtml($"<br><s>{FullPrice}</s>");
            }
        }
    }
}