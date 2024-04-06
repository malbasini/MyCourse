using System.Collections.Generic;
using System.Threading.Tasks;
using MyCourse.Models.InputModels;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Applications
{
    public interface ICourseService
    {
        Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model);
        Task<CourseDetailViewModel> GetCourseAsync(int id);
        Task<List<CourseViewModel>> GetMostRecentCoursesAsync();
        Task<List<CourseViewModel>> GetBestRatingCoursesAsync();
        Task<bool> IsTitleAvailableAsync(string title, int excludeId);
        Task<CourseDetailViewModel> CreateCourseAsync(CourseCreateInputModel inputModel);
    }
}