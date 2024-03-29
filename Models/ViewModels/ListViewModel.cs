using System.Collections.Generic;

namespace MyCourse.Models.ViewModels
{
    public class ListViewModel<T>
    {
        /*--Result contiene al massimo 10 corsi, ed è quello che già
         ci stava restituendo, in TotalCount ci andranno il numero totale
         dei corsi corrispondenti alla ricerca dell'utente.*/
        public List<T> Results { get; set; }
        public int TotalCount { get; set; }
    }
}