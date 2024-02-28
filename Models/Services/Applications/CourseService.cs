using System;
using System.Collections.Generic;
using MyCourse.Models.Enums;
using MyCourse.Models.ValueObjects;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Applications;

public class CourseService
{
    /*
     * Il codice C# fornito è una definizione di funzione con il nome
     * "GetCourses". Appartiene alla classe "CourseService" e restituisce
     * un elenco di oggetti "CourseViewModel".
       
       Ecco una spiegazione riga per riga del codice:
       
       1. `public List<CourseViewModel> GetCourses()`: 
       questa è la dichiarazione della funzione, specificando che 
       la funzione `GetCourses` non accetta alcun parametro e 
       restituisce un elenco di oggetti `CourseViewModel`.
       
       2. `var courseList = new List<CourseViewModel>();`: 
       questa riga crea un nuovo elenco vuoto di oggetti `CourseViewModel`.
       
       3. `var rand = new Random();`: 
       qui viene creato un nuovo oggetto Random. 
       Verrà utilizzato per generare numeri casuali più avanti nello script.
       
       4. Un ciclo "for" inizia con "for (int i = 1; i <= 20; i++)". 
       Questo ciclo verrà eseguito venti volte.
       
       5. In ogni iterazione, viene generato un prezzo casuale, 
       un nuovo "CourseViewModel" viene creato e popolato con dati, 
       quindi aggiunto a "courseList".
       
       6. `return courseList;`: la funzione termina restituendo la 
       `courseList` finale.
       
       La classe "CourseViewModel" rappresenta un corso con proprietà 
       come "Id", "Titolo", "ImagePath", "Autore", "Rating", "FullPrice" 
       e "CurrentPrice".
       
       Il `CourseService` viene chiamato da `CoursesController` nel suo metodo
        `Index` per ottenere l'elenco dei corsi, che vengono poi passati 
        alla vista.
       
       Infine, "Money" è una classe che rappresenta un valore monetario 
       e "Currency" è un enumeratore che elenca tutti i possibili tipi 
       di valuta. "Money" viene utilizzato come tipo di proprietà 
       di "FullPrice" e "CurrentPrice" in "CourseViewModel". 
     */
    public List<CourseViewModel> GetCourses()
    {
        var courseList = new List<CourseViewModel>();
        var rand = new Random();
        for (int i = 1; i <= 20; i++)
        {
            var price = Convert.ToDecimal(rand.NextDouble() * 10 + 10);
            var course = new CourseViewModel
            {
                Id = i,
                Title = $"Corso {i}",
                CurrentPrice = new Money(Currency.EUR, price),
                FullPrice = new Money(Currency.EUR, rand.NextDouble() > 0.5 ? price : price + 1),
                Author = "Nome cognome",
                Rating = rand.Next(10, 50) / 10.0,
                ImagePath = "/logo.svg"
            };
            courseList.Add(course);
        }
        return courseList;
    }
}