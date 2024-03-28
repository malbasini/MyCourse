using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Customizations.ModelBinders;
using MyCourse.Models.Options;

namespace MyCourse.Models.InputModels
{
    [ModelBinder(BinderType = typeof(CourseListInputModelBinder))]
    public class CourseListInputModel
    {
        /*--La classe CourseListInputModel viene valorizzata dal
         Model Binder. In questa classe mettiamo tutta la logica di sanitizzazione*/
        public CourseListInputModel(string search, int page, string orderby, bool ascending, int limit, CoursesOrderOptions orderOptions)
        {
            if (!orderOptions.Allow.Contains(orderby))
            {
                orderby = orderOptions.By;
                ascending = orderOptions.Ascending;
            }

            Search = search ?? "";
            /*--L'istruzione sotto significa che se dovesse arrivare un numero
                0 di pagina o negativo (l'url è stato modificato). Se ad esempio
                page vale -20 il MAX tra 1 e page è 1 e quindi page verrà impostata
                a 1, altrimenti se vale, 2,3,4 etc prendi quei valori.*/
            Page = Math.Max(1, page);
            Limit = Math.Max(1, limit);
            OrderBy = orderby;
            Ascending = ascending;
            /*--Se voglio la terza pagina Page vale (3 -1) * 10 = 20
            quindi come vedevamo nella slide OFFSET = 20--*/
            Offset = (Page - 1) * Limit;
        }
        public string Search { get; }
        public int Page { get; }
        public string OrderBy { get; }
        public bool Ascending { get; }
        
        public int Limit { get; }
        public int Offset { get; }
    }
}