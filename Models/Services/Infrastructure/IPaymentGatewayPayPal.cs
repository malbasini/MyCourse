using System.Threading.Tasks;
using MyCourse.Models.InputModels.Courses;

namespace MyCourse.Models.Services.Infrastructure;

public interface IPaymentGatewayPayPal
{
    Task<string> GetPaymentUrlAsyncPayPal(CoursePayInputModel inputModel);
    Task<CourseSubscribeInputModel> CapturePaymentAsyncPayPal(string token);
}