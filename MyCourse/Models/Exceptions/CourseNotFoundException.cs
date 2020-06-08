
using System;
namespace MyCourse.Models.Exceptions
{
    public class CourseNotFoundException : Exception
    {
        public CourseNotFoundException(int id) : base($"Corso {id} non trovato.")
        {
            
        }
    }
}