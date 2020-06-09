using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MyCourse.Models.ViewModels;
using MyCourse.Models.Services.Application;
using Microsoft.Extensions.Options;
using MyCourse.Models.Options;

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
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(second)); //Esercizio: provate a recuperare il valore 60 usando il servizio di configurazione
                return courseService.GetCourseAsync(id);
            });
        }
        public Task<List<CourseViewModel>> GetCoursesAsync()
        {
            return memoryCache.GetOrCreateAsync($"Courses", cacheEntry =>
            {
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
                return courseService.GetCoursesAsync();
            });
        }

    }
}