using Microsoft.AspNetCore.Mvc;
using MyCourse.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.InputModels
{
    public class CourseCreateInputModel
    {
        [Required(ErrorMessage = "Il titolo è obbligatorio"),
        MinLength(10, ErrorMessage = "Il titolo dev'essere di almeno {1} caratteri"),
        MaxLength(100, ErrorMessage = "Il titolo dev'essere di al massimo {1} caratteri"),
        RegularExpression(@"^[\w\s\.']+$", ErrorMessage = "Titolo non valido")]
        [Remote(action:nameof(CoursesController.IsTitleAvailable),controller:"Courses",ErrorMessage ="Titolo già esistente")]
        public string Title { get; set; }
    }
}
