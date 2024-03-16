using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels;


namespace MyCourse.Models.Services.Application;
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
    public async Task<CourseDetailViewModel> GetCourseAsync(int id)
    {
        /*--Abbiamo due query, questo lo possiamo fare in quanto l'oggetto SqliteCommand
         è in grado di inviare contemporaneamente due query al database.*/
        FormattableString query = $@"SELECT Id, Title, Description, ImagePath, Author, Rating, FullPrice_Amount, 
              FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Courses WHERE Id={id}
            ; SELECT Id, Title, Description, Duration FROM Lessons WHERE CourseId={id}";

        /*--Questa volta avendo inviato due query, una sulla tabella Courses e una sulla tabella Lessons
         mi aspetto che il DataSet contenga due oggetti DataTable.*/
        DataSet dataSet = await db.QueryAsync(query);

        //Course
        var courseTable = dataSet.Tables[0];
        /*--Se il numero di righè è diverso da 1 vuol dire che c'è qualche problema, ad esempio è stato fornito
        un id sbagilato*/
        if (courseTable.Rows.Count != 1) {
            throw new InvalidOperationException($"Did not return exactly 1 row for Course {id}");
        }
        //Una volta fatto il mapping del corso che ritorna indietro un CourseDetailViewModel ci interessiamo alle lezioni.
        var courseRow = courseTable.Rows[0];
        var courseDetailViewModel = CourseDetailViewModel.FromDataRow(courseRow);

        //Course lessons. Tra corsi e lezioni c'è una relazione uno-a-molti, un corso ovviamente ha n lezioni.
        //Con un ciclo foreach le iteriamo a le aggiungiamo al courseDetailViewModel.
        var lessonDataTable = dataSet.Tables[1];

        foreach(DataRow lessonRow in lessonDataTable.Rows) {
            LessonViewModel lessonViewModel = LessonViewModel.FromDataRow(lessonRow);
            courseDetailViewModel.Lessons.Add(lessonViewModel);
        }
        return courseDetailViewModel; 
    }

    public async Task<List<CourseViewModel>> GetCoursesAsync()
    {
        FormattableString query =
            $"SELECT Id, Title, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency,CurrentPrice_Amount, CurrentPrice_Currency FROM Courses";
        DataSet dataSet = await db.QueryAsync(query);
        var dataTable = dataSet.Tables[0];
        var courseList = new List<CourseViewModel>();
        foreach(DataRow courseRow in dataTable.Rows) {
            CourseViewModel courseViewModel = CourseViewModel.FromDataRow(courseRow);
            courseList.Add(courseViewModel);
        }
        return courseList;
    }
}