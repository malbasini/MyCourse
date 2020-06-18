using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.InputModels
{
    public class CourseCreateInputModel
    {
        [Required,
        MinLength(10),
        MaxLength(100),
        RegularExpression(@"^[\w\s.]+$")]
        public string Title { get; set; }
    }
}
