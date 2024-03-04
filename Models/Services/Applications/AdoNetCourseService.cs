using System.Collections.Generic;
using MyCourse.Models.ViewModels;
using System.Data;
using MyCourse.Models.Services.Infrastructure;

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
        string query = "SELECT * FROM Courses;";
        DataSet dataSet = db.Query(query);
        throw new System.NotImplementedException();
    }

    public CourseDetailViewModel GetCourse(int id)
    {
        throw new System.NotImplementedException();
    }
}