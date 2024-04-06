using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCourse.Models.Exceptions;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels;
using MyCourse.Models.ValueObjects;
using MyCourse.Models.InputModels;
using Microsoft.Data.Sqlite;

namespace MyCourse.Models.Services.Applications;
/*Se il nostro servizio applicativo vuole valorizzare i ViewModel dovrà interagire
 con un servizio infrastrutturale che a sua volta accede alle classi di ADO.NET 
 per estrarre le informazioni nel database.*/
public class AdoNetCourseService : ICourseService
{
    /*--Abbiamo visto in precedenza che le dipendenze si impostano nel
     costruttore tramite una interfaccia.*/
    private readonly IDatabaseAccessor db;
    private readonly ILogger<AdoNetCourseService> logger;
    private readonly IOptionsMonitor<CoursesOptions> courseOptionsMonitor;

    public AdoNetCourseService(ILogger<AdoNetCourseService> logger, IDatabaseAccessor db, IOptionsMonitor<CoursesOptions> courseOptionsMonitor)
    {
        this.logger = logger;
        this.courseOptionsMonitor = courseOptionsMonitor;
        this.db = db;
    }
    public async Task<CourseDetailViewModel> GetCourseAsync(int id)
    {
        /*Logging strutturato, dati e testo non vengono mischiati ma tenuti separati*/
        logger.LogInformation("Course {id} requested",id);
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
            logger.LogWarning("Course {id} not found",id);
            throw new CourseNotFoundException(id);
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
    public async Task<List<CourseViewModel>> GetBestRatingCoursesAsync()
    {
        CourseListInputModel inputModel = new CourseListInputModel(
            search: "",
            page: 1,
            orderby: "Rating",
            ascending: false,
            limit: courseOptionsMonitor.CurrentValue.InHome,
            orderOptions: courseOptionsMonitor.CurrentValue.Order);

        ListViewModel<CourseViewModel> result = await GetCoursesAsync(inputModel);
        return result.Results;
    }

    public async Task<bool> IsTitleAvailableAsync(string title, int excludeId)
    {
        DataSet result = await db.QueryAsync($"SELECT COUNT(*) FROM Courses WHERE Title LIKE {title} AND id<>{excludeId}");
        bool titleAvailable = Convert.ToInt32(result.Tables[0].Rows[0][0]) == 0;
        return titleAvailable;
    }

    public async Task<CourseDetailViewModel> CreateCourseAsync(CourseCreateInputModel inputModel)
    {
        string title = inputModel.Title;
        string author = "Mario Rossi";

        try
        {
            DataSet dataSet = await db.QueryAsync($@"INSERT INTO Courses (Title, Author, ImagePath, CurrentPrice_Currency, CurrentPrice_Amount, FullPrice_Currency, FullPrice_Amount) VALUES ({title}, {author}, '/Courses/default.png', 'EUR', 0, 'EUR', 0);
                                                 SELECT last_insert_rowid();");

            int courseId = Convert.ToInt32(dataSet.Tables[0].Rows[0][0]);
            CourseDetailViewModel course = await GetCourseAsync(courseId);
            return course;
        }
        catch (SqliteException exc) when (exc.SqliteErrorCode == 19)
        {
            throw new CourseTitleUnavailableException(title, exc);
        }
    }

    public async Task<List<CourseViewModel>> GetMostRecentCoursesAsync()
    {
        CourseListInputModel inputModel = new CourseListInputModel(
            search: "",
            page: 1,
            orderby: "Id",
            ascending: false,
            limit: courseOptionsMonitor.CurrentValue.InHome,
            orderOptions: courseOptionsMonitor.CurrentValue.Order);

        ListViewModel<CourseViewModel> result = await GetCoursesAsync(inputModel);
        return result.Results;
    }

    public async Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model)
    {
        string orderby = model.OrderBy == "CurrentPrice" ? "CurrentPrice_Amount" : model.OrderBy;
        string direction = model.Ascending ? "ASC" : "DESC";
                                    
        FormattableString query = $@"SELECT Id, Title, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Courses WHERE Title LIKE {"%" + model.Search + "%"} ORDER BY {(Sql) orderby} {(Sql) direction} LIMIT {model.Limit} OFFSET {model.Offset}; 
            SELECT COUNT(*) FROM Courses WHERE Title LIKE {"%" + model.Search + "%"}";
        DataSet dataSet = await db.QueryAsync(query);
        var dataTable = dataSet.Tables[0];
        var courseList = new List<CourseViewModel>();
        foreach (DataRow courseRow in dataTable.Rows)
        {
            CourseViewModel courseViewModel = CourseViewModel.FromDataRow(courseRow);
            courseList.Add(courseViewModel);
        }

        ListViewModel<CourseViewModel> result = new ListViewModel<CourseViewModel>
        {
            Results = courseList,
            TotalCount = Convert.ToInt32(dataSet.Tables[1].Rows[0][0])
        };

        return result;
    }
}