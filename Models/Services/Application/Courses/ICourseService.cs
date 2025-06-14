using System.Collections.Generic;
using System.Threading.Tasks;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.ViewModels;
using MyCourse.Models.ViewModels.Courses;

namespace MyCourse.Models.Services.Application.Courses
{
    public interface ICourseService
    {
        Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model);
        Task<CourseDetailViewModel> GetCourseAsync(int id);
        Task<List<CourseViewModel>> GetMostRecentCoursesAsync();
        Task<List<CourseViewModel>> GetBestRatingCoursesAsync();
        Task<CourseEditInputModel> GetCourseForEditingAsync(int id);
        Task<CourseDetailViewModel> CreateCourseAsync(CourseCreateInputModel inputModel);
        Task<CourseDetailViewModel> EditCourseAsync(CourseEditInputModel inputModel);
        Task DeleteCourseAsync(CourseDeleteInputModel inputModel);
        Task<bool> IsTitleAvailableAsync(string title, int excludeId);
        Task SendQuestionToCourseAuthorAsync(int id, string question);
        Task<string> GetCourseAuthorIdAsync(int courseId);
        Task<int> GetCourseCountByAuthorIdAsync(string? authorId);
        Task SubscribeCourseAsync(CourseSubscribeInputModel inputModel);
        Task<bool> IsCourseSubscribedAsync(int courseId);
        Task<bool> IsCourseSubscribedAsync(int courseId, string userId);
        Task<CourseSubscribeInputModel> CapturePaymentAsyncStripe(int id, string token);
        Task<CourseSubscribeInputModel> CapturePaymentAsyncPayPal(int id, string token);
        Task<int?> GetCourseVoteAsync(int id);
        Task VoteCourseAsync(CourseVoteInputModel inputModel);
        Task<string> GetPaymentUrlAsyncPayPal(int id);
        Task<string> GetPaymentUrlAsyncStripe(int id);
    }

    
}