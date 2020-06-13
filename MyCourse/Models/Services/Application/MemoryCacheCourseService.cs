using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MyCourse.Models.ViewModels;
using MyCourse.Models.Services.Application;
using Microsoft.Extensions.Options;
using MyCourse.Models.Options;
using MyCourse.Models.InputModels;

namespace MyCourse.Models.Services.Application
{
    public class MemoryCacheCourseService : ICachedCourseService
    {
        private readonly ICourseService courseService;
        private readonly IMemoryCache memoryCache;
        private readonly IOptionsMonitor<TimeFromSecondExpireCacheOptions> memoryCacheCourseServiceOptions;
        public MemoryCacheCourseService(IOptionsMonitor<TimeFromSecondExpireCacheOptions> memoryCacheCourseServiceOptions, ICourseService courseService, IMemoryCache memoryCache)
        {
            this.memoryCacheCourseServiceOptions = memoryCacheCourseServiceOptions;
            this.courseService = courseService;
            this.memoryCache = memoryCache;
        }
        public Task<CourseDetailViewModel> GetCourseAsync(int id)
        {
            return memoryCache.GetOrCreateAsync($"Course{id}", cacheEntry =>
            {
                int second = memoryCacheCourseServiceOptions.CurrentValue.Default;
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(second)); 
                return courseService.GetCourseAsync(id);
            });
        }

        Task<ListViewModel<CourseViewModel>> ICourseService.GetCoursesAsync(CourseListInputModel model)
        {
            //Metto in cache i risultati solo per le prime 5 pagine del catalogo, che reputo essere
            //le più visitate dagli utenti, e che perciò mi permettono di avere il maggior beneficio dalla cache.
            //E inoltre, metto in cache i risultati solo se l'utente non ha cercato nulla.
            //In questo modo riduco drasticamente il consumo di memoria RAM
            bool canCache = model.Page <= 5 && string.IsNullOrEmpty(model.Search);
            
            //Se canCache è true, sfrutto il meccanismo di caching
            if (canCache)
            {
                return memoryCache.GetOrCreateAsync($"Courses{model.Page}-{model.OrderBy}-{model.Ascending}", cacheEntry => 
                {
                    int second = memoryCacheCourseServiceOptions.CurrentValue.Default;
                    cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(second));
                    return courseService.GetCoursesAsync(model);
                });
            }

            //Altrimenti uso il servizio applicativo sottostante, che recupererà sempre i valori dal database
            return courseService.GetCoursesAsync(model);
        }

        public Task<List<CourseViewModel>> GetBestRatingCoursesAsync()
        {
            return memoryCache.GetOrCreateAsync($"BestRatingCourses", cacheEntry => 
            {
                int second = memoryCacheCourseServiceOptions.CurrentValue.Default;
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(second));
                return courseService.GetBestRatingCoursesAsync();
            });
        }
        public Task<List<CourseViewModel>> GetMostRecentCoursesAsync()
        {
            return memoryCache.GetOrCreateAsync($"MostRecentCourses", cacheEntry => 
            {
                int second = memoryCacheCourseServiceOptions.CurrentValue.Default;
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(second));
                return courseService.GetMostRecentCoursesAsync();
            });
        }
    }
}