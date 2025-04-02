using System.Threading.Tasks;

namespace MyCourse.Models.Services.Application.Courses
{
    public interface ICachedCourseService : ICourseService
    {
        Task<string> GetCourseAuthorIdAsync(int courseId);
        Task<int> GetCourseCountByAuthorIdAsync(string? userId);
        Task<bool> IsCourseSubscribedAsync(int courseId, string? userId);
        Task<string> GetPaymentUrlAsyncPayPal(int id);
        Task<string> GetPaymentUrlAsyncStripe(int id);
    }
}