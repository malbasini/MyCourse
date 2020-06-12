using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels;
using MyCourse.Models.Exceptions;
using System.Linq;
using MyCourse.Models.ValueTypes;

namespace MyCourse.Models.Services.Application
{
    public class AdoNetCourseService : ICourseService
    {
        private readonly IDatabaseAccessor db;
        private readonly IOptionsMonitor<CoursesOptions> courseOptions;
        private readonly ILogger<AdoNetCourseService> logger;
        public AdoNetCourseService(ILogger<AdoNetCourseService> logger, IDatabaseAccessor db, IOptionsMonitor<CoursesOptions> courseOptions)
        {
            this.logger = logger;
            this.courseOptions = courseOptions;
            this.db = db;
        }
        public async Task<CourseDetailViewModel> GetCourseAsync(int id)
        {
            logger.LogInformation("Corso {id} richiesto.",id);

            FormattableString query = $@"SELECT Id, Title, Description, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Courses WHERE Id={id}
            ; SELECT Id, Title, Description, Duration FROM Lessons WHERE CourseId={id}";

            DataSet dataSet = await db.QueryAsync(query);

            //Course
            var courseTable = dataSet.Tables[0];
            if (courseTable.Rows.Count != 1)
            {
                logger.LogWarning("Corso {id} non trovato",id);
                throw new CourseNotFoundException(id);
            }
            var courseRow = courseTable.Rows[0];
            var courseDetailViewModel = CourseDetailViewModel.FromDataRow(courseRow);

            //Course lessons
            var lessonDataTable = dataSet.Tables[1];

            foreach (DataRow lessonRow in lessonDataTable.Rows)
            {
                LessonViewModel lessonViewModel = LessonViewModel.FromDataRow(lessonRow);
                courseDetailViewModel.Lessons.Add(lessonViewModel);
            }
            return courseDetailViewModel;
        }

        public async Task<List<CourseViewModel>> GetCoursesAsync(string search, int page, string orderby, bool ascending)
        {
            page = Math.Max(1,page);
            int limit = courseOptions.CurrentValue.PerPage;
            int offset = (page -1)*limit;
            var orderOptions = courseOptions.CurrentValue.Order;
            if(!orderOptions.Allow.Contains(orderby)) 
            {
                orderby = orderOptions.By; 
                ascending = orderOptions.Ascending;   
            }
            if(orderby=="CurrentPrice")
                orderby="CurrentPrice_Amount"; 
            string direction = ascending ? "ASC" : "DESC";     
            List<CourseViewModel> courseList = new List<CourseViewModel>();
            FormattableString query = $"SELECT Id,Title,ImagePath,Author,Rating,FullPrice_Amount,FullPrice_Currency,CurrentPrice_Amount,CurrentPrice_Currency FROM Courses WHERE Title LIKE {"%" + search + "%"} ORDER BY {(Sql)orderby} {(Sql)direction} LIMIT {limit} OFFSET {offset}";
            DataSet dataSet = await db.QueryAsync(query);
            DataTable dataTable = dataSet.Tables[0];
            foreach (DataRow courseRow in dataTable.Rows)
            {
                CourseViewModel course = CourseViewModel.FromDataRow(courseRow);
                courseList.Add(course);
            }
            return courseList;
        }
    }
}