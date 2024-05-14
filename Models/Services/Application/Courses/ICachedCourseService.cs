using System.Threading.Tasks;

namespace MyCourse.Models.Services.Application.Courses
{
    public interface ICachedCourseService : ICourseService
    {
        Task<string> GetCourseAuthorIdAsync(int courseId);
        Task<int> GetCourseCountByAuthorIdAsync(string? userId);
    }
}