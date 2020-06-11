using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application
{
    public class EFCoreCourseService : ICourseService
    {
        private readonly MyCourseDbContext dbContext;
        public EFCoreCourseService(MyCourseDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<CourseDetailViewModel> GetCourseAsync(int id)
        {
              CourseDetailViewModel viewModel = await dbContext.Courses
              .AsNoTracking()
              .Where(course => course.Id == id)
              .Select(course => new CourseDetailViewModel{
                        Id=course.Id,
                        Title=course.Title,
                        Author=course.Author,
                        Rating=course.Rating,
                        CurrentPrice=course.CurrentPrice,
                        FullPrice=course.FullPrice,
                        ImagePath=course.ImagePath,
                        Description=course.Description,
                        Lessons=course.Lessons.Select(lessons => new LessonViewModel{
                            Id=lessons.Id,
                            Description=lessons.Description,
                            Duration=lessons.Duration,
                            Title=lessons.Title
                        }).ToList()
                })
                               //.FirstOrDefaultAsync(); //Restituisce null se l'elenco è vuoto e non solleva mai un'eccezione
                               //.SingleOrDefaultAsync(); //Tollera il fatto che l'elenco sia vuoto e in quel caso restituisce null, oppure se l'elenco contiene più di 1 elemento, solleva un'eccezione
                               //.FirstAsync(); //Restituisce il primo elemento, ma se l'elenco è vuoto solleva un'eccezione
                               //.SingleAsync(); //Restituisce il primo elemento, ma se l'elenco è vuoto o contiene più di un elemento, solleva un'eccezione
                .SingleAsync();//Metodo che accede al Database. Se l'elenco contiene 0 o più di un'occorrenza solleva un'eccezione.
              return viewModel;
        }
        public async Task<List<CourseViewModel>> GetCoursesAsync(string search)
        {
            search = search ?? "";
            IQueryable<CourseViewModel> queryLinq = dbContext.Courses.AsNoTracking().Select(course => new CourseViewModel
            { 
                Id=course.Id,
                Title=course.Title,
                Author=course.Author,
                Rating=course.Rating,
                CurrentPrice=course.CurrentPrice,
                FullPrice=course.FullPrice,
                ImagePath=course.ImagePath
            }).Where(course=>course.Title.Contains(search));
            List<CourseViewModel> courses = await queryLinq.ToListAsync();//La query al database viene invocata quì.
            return courses;
        }
    }
}