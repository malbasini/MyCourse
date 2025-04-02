using System.Threading.Tasks;
using MyCourse.Models.InputModels.Courses;

namespace MyCourse.Models.Services.Infrastructure
{
    public interface IPaymentGatewayStripe
    {
        Task<string> GetPaymentUrlAsyncStripe(CoursePayInputModel inputModel);
        Task<CourseSubscribeInputModel> CapturePaymentAsyncStripe(string token);
    }
}