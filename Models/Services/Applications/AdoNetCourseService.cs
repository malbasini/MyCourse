using System.Collections.Generic;
using MyCourse.Models.ViewModels;
using System.Data;
using MyCourse.Models.Services.Infrastructure;
using System;
using MyCourse.Models.Enums;
using MyCourse.Models.ValueObjects;



namespace MyCourse.Models.Services.Applications;
/*Se il nostro servizio applicativo vuole valorizzare i ViewModel dovrà interagire
 con un servizio infrastrutturale che a sua volta accede alle classi di ADO.NET 
 per estrarre le informazioni nel database.*/
public class AdoNetCourseService : ICourseService
{
    /*--Abbiamo visto in precedenza che le dipendenze si impostano nel
     costruttore tramite una interfaccia.*/
    private readonly IDatabaseAccessor db;
    public AdoNetCourseService(IDatabaseAccessor db)
    {
        this.db = db;
    }
    public List<CourseViewModel> GetCourses()
    {
        string query = "SELECT Id,Title,ImagePath,Author,Rating," +
                       "FullPrice_Amount,FullPrice_Currency," +
                       "CurrentPrice_Amount,CurrentPrice_Currency FROM Courses;";
        DataSet dataSet = db.Query(query);
        DataTable dataTable = dataSet.Tables[0];
        List<CourseViewModel> courses = new List<CourseViewModel>();
        foreach (DataRow row in dataTable.Rows)
        {
            var courseRow = CourseViewModel.FromDataRow(row);
            courses.Add(courseRow);
        }
        return courses;
    }

    public CourseDetailViewModel GetCourse(int id)
    {
        throw new System.NotImplementedException();
    }
}