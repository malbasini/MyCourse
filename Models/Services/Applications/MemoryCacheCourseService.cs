using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MyCourse.Models.Options;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application
{
    public class MemoryCacheCourseService : ICachedCourseService
    {
        private readonly ICourseService courseService;
        private readonly IMemoryCache memoryCache;
        private readonly IOptionsMonitor<IMemoryCacheOptions> memoryCacheOptions;
        public MemoryCacheCourseService(ICourseService courseService, IMemoryCache memoryCache, IOptionsMonitor<IMemoryCacheOptions> memoryCacheOptions)
        {
            this.courseService = courseService;
            this.memoryCache = memoryCache;
            this.memoryCacheOptions = memoryCacheOptions;
        }

        //TODO: ricordati di usare memoryCache.Remove($"Course{id}") quando aggiorni il corso

        public Task<CourseDetailViewModel> GetCourseAsync(int id)
        {
            return memoryCache.GetOrCreateAsync($"Course{id}", cacheEntry => 
            {
                cacheEntry.SetSize(1); //Da usare se si è impostato un limite di cache
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(memoryCacheOptions.CurrentValue.FromSecond)); //Esercizio: provate a recuperare il valore 60 usando il servizio di configurazione
                return courseService.GetCourseAsync(id);
            });
        }

        public Task<List<CourseViewModel>> GetCoursesAsync(string search)
        {
            return memoryCache.GetOrCreateAsync($"Courses{search}", cacheEntry => 
            {
                cacheEntry.SetSize(1); //Da usare se si è impostato un limite di cache
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(memoryCacheOptions.CurrentValue.FromSecond));
                return courseService.GetCoursesAsync(search);
            });
        }
    }
}