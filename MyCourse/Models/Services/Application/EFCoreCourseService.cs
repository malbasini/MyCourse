using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyCourse.Models.Entities;
using MyCourse.Models.InputModels;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application
{
    public class EFCoreCourseService : ICourseService
    {
        private readonly MyCourseDbContext dbContext;
        private readonly IOptionsMonitor<CoursesOptions> courseOptions;
        public EFCoreCourseService(MyCourseDbContext dbContext, IOptionsMonitor<CoursesOptions> courseOptions)
        {
            this.courseOptions = courseOptions;
            this.dbContext = dbContext;
        }
        public async Task<CourseDetailViewModel> GetCourseAsync(int id)
        {
            CourseDetailViewModel viewModel = await dbContext.Courses
            .AsNoTracking()
            .Where(course => course.Id == id)
            .Select(course => new CourseDetailViewModel
            {
                Id = course.Id,
                Title = course.Title,
                Author = course.Author,
                Rating = course.Rating,
                CurrentPrice = course.CurrentPrice,
                FullPrice = course.FullPrice,
                ImagePath = course.ImagePath,
                Description = course.Description,
                Lessons = course.Lessons.Select(lessons => new LessonViewModel
                {
                    Id = lessons.Id,
                    Description = lessons.Description,
                    Duration = lessons.Duration,
                    Title = lessons.Title
                }).ToList()
            })
              //.FirstOrDefaultAsync(); //Restituisce null se l'elenco è vuoto e non solleva mai un'eccezione
              //.SingleOrDefaultAsync(); //Tollera il fatto che l'elenco sia vuoto e in quel caso restituisce null, oppure se l'elenco contiene più di 1 elemento, solleva un'eccezione
              //.FirstAsync(); //Restituisce il primo elemento, ma se l'elenco è vuoto solleva un'eccezione
              //.SingleAsync(); //Restituisce il primo elemento, ma se l'elenco è vuoto o contiene più di un elemento, solleva un'eccezione
              .SingleAsync();//Metodo che accede al Database. Se l'elenco contiene 0 o più di un'occorrenza solleva un'eccezione.
            return viewModel;
        }
        public async Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model)
        {
            IQueryable<Course> baseQuery = dbContext.Courses;
            switch(model.OrderBy)
            {
                case "Title":
                    if (model.Ascending)
                        baseQuery = baseQuery.OrderBy(course => course.Title);
                    else
                        baseQuery = baseQuery.OrderByDescending(course => course.Title);
                    break;
                case "Rating":
                    if (model.Ascending)
                        baseQuery = baseQuery.OrderBy(course => course.Rating);
                    else
                        baseQuery = baseQuery.OrderByDescending(course => course.Rating);
                    break;
                case "CurrentPrice":
                    if (model.Ascending)
                        baseQuery = baseQuery.OrderBy(course => course.CurrentPrice.Amount);
                    else
                        baseQuery = baseQuery.OrderByDescending(course => course.CurrentPrice.Amount);
                    break;
                case "Id":
                    if (model.Ascending)
                        baseQuery = baseQuery.OrderBy(course => course.Id);
                    else
                        baseQuery = baseQuery.OrderByDescending(course => course.Id);
                    break;
            }   
            IQueryable<CourseViewModel> queryLinq = baseQuery
            .AsNoTracking()
            .Select(course => new CourseViewModel
            {
                Id = course.Id,
                Title = course.Title,
                Author = course.Author,
                Rating = course.Rating,
                CurrentPrice = course.CurrentPrice,
                FullPrice = course.FullPrice,
                ImagePath = course.ImagePath
            }).Where(course => course.Title.Contains(model.Search));
            List<CourseViewModel> courses = await queryLinq
            .Skip(model.Offset)
            .Take(model.Limit)
            .ToListAsync();//La query al database viene invocata quì.
            int totalCount = await queryLinq.CountAsync();
            ListViewModel<CourseViewModel> result = new ListViewModel<CourseViewModel>
            {
                 Results = courses,
                 TotalCount = totalCount
            }; 
            return result;
        }

       public async Task<List<CourseViewModel>> GetBestRatingCoursesAsync()
        {
            CourseListInputModel inputModel = new CourseListInputModel(
                search: "",
                page: 1,
                orderby: "Rating",
                ascending: false,
                limit: courseOptions.CurrentValue.InHome,
                orderOptions: courseOptions.CurrentValue.Order);

            ListViewModel<CourseViewModel> result = await GetCoursesAsync(inputModel);
            return result.Results;
        }

        public async Task<List<CourseViewModel>> GetMostRecentCoursesAsync()
        {
            CourseListInputModel inputModel = new CourseListInputModel(
                search: "",
                page: 1,
                orderby: "Id",
                ascending: false,
                limit: courseOptions.CurrentValue.InHome,
                orderOptions: courseOptions.CurrentValue.Order);

            ListViewModel<CourseViewModel> result = await GetCoursesAsync(inputModel);
            return result.Results;
        }
    }
}