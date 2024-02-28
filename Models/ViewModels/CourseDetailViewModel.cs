using System;
using System.Collections.Generic;
using System.Linq;
using MyCourse.Models.ValueObjects;

namespace MyCourse.Models.ViewModels
{
    /*
     * Il codice C# fornito definisce una classe "CourseDetailViewModel",
     * che viene utilizzata come ViewModel in un contesto ASP.NET MVC.La classe "CourseDetailViewModel"
     * è derivata da "CourseViewModel", il che significa che eredita tutti i membri
     * (ad esempio metodi, proprietà, ecc.) di quella classe e può aggiungerli o sovrascriverli
     * secondo necessità.
     */
    public class CourseDetailViewModel : CourseViewModel
    {
        /*
         * Si tratta di una proprietà implementata automaticamente denominata "Descrizione"
         * di tipo "stringa". Le proprietà implementate automaticamente soddisfano il requisito .NET
         * secondo cui l'accesso alle proprietà (sia il recupero che l'impostazione) avviene tramite metodi.
         */
        public string Description { get; set; }
        /*
         * Si tratta di una proprietà implementata automaticamente denominata "Lessons" di tipo "List<LessonViewModel>".
         * Viene utilizzato per contenere una raccolta di lezioni associate al corso.
         */
        public List<LessonViewModel> Lessons { get; set; }
        
        /*
         * Questa proprietà è di tipo "TimeSpan" e dispone di un getter che calcola la durata totale
         * di tutte le lezioni del corso. Utilizza LINQ per calcolare la somma della proprietà
         * "Duration" di tutti gli oggetti "LessonViewModel" nell'elenco "Lessons" (in secondi)
         * e quindi riconverte questo totale in un "TimeSpan". Se "Lessons" è nullo,
         * il valore predefinito è "TimeSpan" pari a zero.
         */
        public TimeSpan TotalCourseDuration
        {
            get => TimeSpan.FromSeconds(Lessons?.Sum(l => l.Duration.TotalSeconds) ?? 0);
        }
    }
}