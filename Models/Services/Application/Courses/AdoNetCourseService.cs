using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Ganss.Xss;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCourse.Controllers;
using MyCourse.Models.Enums;
using MyCourse.Models.Exceptions.Application;
using MyCourse.Models.Exceptions.Infrastructure;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ValueObjects;
using MyCourse.Models.ViewModels;
using MyCourse.Models.ViewModels.Courses;
using MyCourse.Models.ViewModels.Lessons;

namespace MyCourse.Models.Services.Application.Courses
{
    public class AdoNetCourseService : ICourseService
    {
        private readonly ILogger<AdoNetCourseService> logger;
        private readonly IDatabaseAccessor db;
        private readonly IOptionsMonitor<CoursesOptions> coursesOptions;
        private readonly IImagePersister imagePersister;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IOptionsMonitor<SmtpOptions> smtp;
        private readonly ILogger<MailKitEmailSender> loggerMailKit;
        private readonly IConfiguration configuration;
        private readonly LinkGenerator linkGenerator;
        private readonly IPaymentGatewayStripe paymentGatewayStripe;
        private readonly IPaymentGatewayPayPal paymentGatewayPayPal;
        public AdoNetCourseService(
            IConfiguration configuration, 
            IOptionsMonitor<SmtpOptions> smtp,
            ILogger<MailKitEmailSender> loggerMailKit, 
            IHttpContextAccessor httpContextAccessor, 
            ILogger<AdoNetCourseService> logger, 
            IDatabaseAccessor db, 
            IImagePersister imagePersister, 
            IOptionsMonitor<CoursesOptions> coursesOptions,
            IPaymentGatewayStripe paymentGatewayStripe,
            IPaymentGatewayPayPal paymentGatewayPayPal,
            LinkGenerator linkGenerator)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.imagePersister = imagePersister;
            this.coursesOptions = coursesOptions;
            this.logger = logger;
            this.db = db;
            this.smtp = smtp;
            this.loggerMailKit = loggerMailKit;
            this.configuration = configuration;
            this.paymentGatewayStripe = paymentGatewayStripe;
            this.paymentGatewayPayPal = paymentGatewayPayPal;
            this.linkGenerator = linkGenerator;
        }
        public async Task<CourseDetailViewModel> GetCourseAsync(int id)
        {
            logger.LogInformation("Course {id} requested", id);

            FormattableString query = $@"SELECT Id, Title, Description, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Courses WHERE Id={id} AND Status<>{nameof(CourseStatus.Deleted)}
            ; SELECT Id, Title, Description, Duration FROM Lessons WHERE CourseId={id} ORDER BY [Order], Id";

            DataSet dataSet = await db.QueryAsync(query);

            //Course
            var courseTable = dataSet.Tables[0];
            if (courseTable.Rows.Count != 1)
            {
                logger.LogWarning("Course {id} not found", id);
                throw new CourseNotFoundException(id);
            }
            var courseRow = courseTable.Rows[0];
            var courseDetailViewModel = CourseDetailViewModel.FromDataRow(courseRow);

            //Course lessons
            var lessonDataTable = dataSet.Tables[1];

            foreach (DataRow lessonRow in lessonDataTable.Rows)
            {
                LessonViewModel lessonViewModel = LessonViewModel.FromDataRow(lessonRow);
                courseDetailViewModel.Lessons.Add(lessonViewModel);
            }
            return courseDetailViewModel;
        }
        public async Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model)
        {
            string orderby = model.OrderBy == "CurrentPrice" ? "CurrentPrice_Amount" : model.OrderBy;
            string direction = model.Ascending ? "ASC" : "DESC";

            FormattableString query = $@"SELECT Id, Title, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Courses WHERE Title LIKE {"%" + model.Search + "%"} AND Status<>{nameof(CourseStatus.Deleted)} ORDER BY {(Sql)orderby} {(Sql)direction} LIMIT {model.Limit} OFFSET {model.Offset}; 
            SELECT COUNT(*) FROM Courses WHERE Title LIKE {"%" + model.Search + "%"} AND Status<>{nameof(CourseStatus.Deleted)}";
            DataSet dataSet = await db.QueryAsync(query);
            var dataTable = dataSet.Tables[0];
            var courseList = new List<CourseViewModel>();
            foreach (DataRow courseRow in dataTable.Rows)
            {
                CourseViewModel courseViewModel = CourseViewModel.FromDataRow(courseRow);
                courseList.Add(courseViewModel);
            }

            ListViewModel<CourseViewModel> result = new ListViewModel<CourseViewModel>
            {
                Results = courseList,
                TotalCount = Convert.ToInt32(dataSet.Tables[1].Rows[0][0])
            };

            return result;
        }
        public async Task<CourseEditInputModel> GetCourseForEditingAsync(int id)
        {
            FormattableString query = $@"SELECT Id, Title, Description, ImagePath, Email, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency, RowVersion FROM Courses WHERE Id={id} AND Status<>{nameof(CourseStatus.Deleted)}";

            DataSet dataSet = await db.QueryAsync(query);

            var courseTable = dataSet.Tables[0];
            if (courseTable.Rows.Count != 1)
            {
                logger.LogWarning("Course {id} not found", id);
                throw new CourseNotFoundException(id);
            }
            var courseRow = courseTable.Rows[0];
            var courseEditInputModel = CourseEditInputModel.FromDataRow(courseRow);
            return courseEditInputModel;
        }
        public async Task<List<CourseViewModel>> GetBestRatingCoursesAsync()
        {
            CourseListInputModel inputModel = new CourseListInputModel(
                search: "",
                page: 1,
                orderby: "Rating",
                ascending: false,
                limit: coursesOptions.CurrentValue.InHome,
                orderOptions: coursesOptions.CurrentValue.Order);

            ListViewModel<CourseViewModel> result = await GetCoursesAsync(inputModel);
            return result.Results;
        }
        public async Task<List<CourseViewModel>> GetMostRecentCoursesAsync()
        {
            CourseListInputModel inputModel = new CourseListInputModel(
                search: "",
                page: 1,
                orderby: "Id",
                ascending: false,
                limit: coursesOptions.CurrentValue.InHome,
                orderOptions: coursesOptions.CurrentValue.Order);

            ListViewModel<CourseViewModel> result = await GetCoursesAsync(inputModel);
            return result.Results;
        }
        public async Task<CourseDetailViewModel> CreateCourseAsync(CourseCreateInputModel inputModel)
        {
            string title = inputModel.Title;
            string author = string.Empty;
            string authorId = string.Empty;
            try
            {
                author = httpContextAccessor.HttpContext.User.FindFirst("FullName").Value;
                authorId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
            catch (NullReferenceException)
            {
                throw new UserUnknownException();
            }
            try
            {
                int courseId = await db.QueryScalarAsync<int>($@"INSERT INTO Courses (Title, Author, ImagePath, Rating, CurrentPrice_Currency, CurrentPrice_Amount, FullPrice_Currency, FullPrice_Amount, Status, AuthorId) VALUES ({title}, {author}, '/Courses/default.png', 0, 'EUR', 0, 'EUR', 0, {nameof(CourseStatus.Draft)},{authorId});
                                                 SELECT last_insert_rowid();");

                CourseDetailViewModel course = await GetCourseAsync(courseId);
                return course;
            }
            catch (ConstraintViolationException exc)
            {
                throw new CourseTitleUnavailableException(inputModel.Title, exc);
            }
        }
        public async Task<bool> IsTitleAvailableAsync(string title, int id)
        {
            bool titleExists = await db.QueryScalarAsync<bool>($"SELECT COUNT(*) FROM Courses WHERE Title LIKE {title} AND id<>{id}");
            return !titleExists;
        }

        public async Task SendQuestionToCourseAuthorAsync(int id, string question)
        {
            // Recupero le informazioni del corso
            FormattableString query = $@"SELECT Title, Email FROM Courses WHERE Courses.Id={id}";
            DataSet dataSet = await db.QueryAsync(query);

            if (dataSet.Tables[0].Rows.Count == 0)
            {
                logger.LogWarning("Course {id} not found", id);
                throw new CourseNotFoundException(id);
            }

            string courseTitle = Convert.ToString(dataSet.Tables[0].Rows[0]["Title"]);
            string courseEmail = Convert.ToString(dataSet.Tables[0].Rows[0]["Email"]);

            // Recupero le informazioni dell'utente che vuole inviare la domanda
            string userFullName;
            string userEmail;
            //sanitizer question
            var sanitizer = new HtmlSanitizer();
            question = sanitizer.Sanitize(question);
            try
            {
                userFullName = httpContextAccessor.HttpContext.User.FindFirst("FullName").Value;
                userEmail = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Email).Value;
            }
            catch (NullReferenceException)
            {
                throw new UserUnknownException();
            }
            // Compongo il testo della domanda
            string subject = $@"Domanda per il tuo corso ""{courseTitle}""";
            string message = $@"<p>L'utente {userFullName} (<a href=""{userEmail}"">{userEmail}</a>)
                                ti ha inviato la seguente domanda:</p>
                                <p>{question}</p>";

            // Invio la domanda
            try
            {
                //courseEmail e-mail di Author
                //userEmail e-mail dello studente
                MailKitEmailSender mailClient = new MailKitEmailSender(smtp, loggerMailKit, configuration);
                mailClient.MailFrom = userEmail;
                await mailClient.SendEmailAsync(courseEmail, subject, message);
            }
            catch 
            {
                throw new SendException();
            }
        }


        public async Task<CourseDetailViewModel> EditCourseAsync(CourseEditInputModel inputModel)
        {
            try
            {
                string imagePath = null;
                if (inputModel.Image != null)
                {
                    imagePath = await imagePersister.SaveCourseImageAsync(inputModel.Id, inputModel.Image);
                }
                int affectedRows = await db.CommandAsync($"UPDATE Courses SET ImagePath=COALESCE({imagePath}, ImagePath), Title={inputModel.Title}, Description={inputModel.Description}, Email={inputModel.Email}, CurrentPrice_Currency={inputModel.CurrentPrice.Currency.ToString()}, CurrentPrice_Amount={inputModel.CurrentPrice.Amount}, FullPrice_Currency={inputModel.FullPrice.Currency.ToString()}, FullPrice_Amount={inputModel.FullPrice.Amount} WHERE Id={inputModel.Id} AND Status<>{nameof(CourseStatus.Deleted)} AND RowVersion={inputModel.RowVersion}");
                if (affectedRows == 0)
                {
                    bool courseExists = await db.QueryScalarAsync<bool>($"SELECT COUNT(*) FROM Courses WHERE Id={inputModel.Id} AND Status<>{nameof(CourseStatus.Deleted)}");
                    if (courseExists)
                    {
                        throw new OptimisticConcurrencyException();
                    }
                    else
                    {
                        throw new CourseNotFoundException(inputModel.Id);
                    }
                }
            }
            catch (ConstraintViolationException exc)
            {
                throw new CourseTitleUnavailableException(inputModel.Title, exc);
            }
            catch (ImagePersistenceException exc)
            {
                throw new CourseImageInvalidException(inputModel.Id, exc);
            }

            CourseDetailViewModel course = await GetCourseAsync(inputModel.Id);
            return course;
        }

        public async Task DeleteCourseAsync(CourseDeleteInputModel inputModel)
        {
            int affectedRows = await this.db.CommandAsync($"UPDATE Courses SET Status={nameof(CourseStatus.Deleted)} WHERE Id={inputModel.Id} AND Status<>{nameof(CourseStatus.Deleted)}");
            if (affectedRows == 0)
            {
                throw new CourseNotFoundException(inputModel.Id);
            }
        }
        public Task<string> GetCourseAuthorIdAsync(int courseId)
        {
            return db.QueryScalarAsync<string>($"SELECT AuthorId FROM Courses WHERE Id={courseId}");
        }

        public Task<int> GetCourseCountByAuthorIdAsync(string authorId)
        {
            return db.QueryScalarAsync<int>($"SELECT COUNT(*) FROM Courses WHERE AuthorId={authorId}");
        }

        public async Task SubscribeCourseAsync(CourseSubscribeInputModel inputModel)
        {
            try
            {
                await db.CommandAsync($"INSERT INTO Subscriptions (UserId, CourseId, PaymentDate, PaymentType, Paid_Currency, Paid_Amount, TransactionId) VALUES ({inputModel.UserId}, {inputModel.CourseId}, {inputModel.PaymentDate}, {inputModel.PaymentType}, {inputModel.Paid.Currency.ToString()}, {inputModel.Paid.Amount}, {inputModel.TransactionId})");
            }
            catch (ConstraintViolationException)
            {
                throw new CourseSubscriptionException(inputModel.CourseId);
            }
        }

        public Task<bool> IsCourseSubscribedAsync(int courseId, string? userId)
        {
            return db.QueryScalarAsync<bool>($"SELECT COUNT(*) FROM Subscriptions WHERE CourseId={courseId} AND UserId={userId}");
        }

        public Task<CourseSubscribeInputModel> CapturePaymentAsyncStripe(int id, string token)
        {
            return paymentGatewayStripe.CapturePaymentAsyncStripe(token);
        }

        public Task<CourseSubscribeInputModel> CapturePaymentAsyncPayPal(int id, string token)
        {
            return paymentGatewayPayPal.CapturePaymentAsyncPayPal(token);
        }

        public async Task<string> GetPaymentUrlAsyncPayPal(int courseId)
        {
            CourseDetailViewModel viewModel = await GetCourseAsync(courseId);
            CoursePayInputModel inputModel = null!;
            if (httpContextAccessor.HttpContext != null)
            {
                inputModel = new()
                {
                    CourseId = courseId,
                    UserId = httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Description = viewModel.Title,
                    Price = viewModel.CurrentPrice,
                    ReturnUrl = linkGenerator.GetUriByAction(httpContextAccessor.HttpContext, action:nameof(CoursesController.SubscribePayPal),
                        controller:"Courses",
                        values:new{id=courseId}),
                    CancelUrl = linkGenerator.GetUriByAction(httpContextAccessor.HttpContext, action:nameof(CoursesController.Detail),
                        controller:"Courses",
                        values:new{id=courseId})
                };
            }

            Debug.Assert(inputModel != null, nameof(inputModel) + " != null");
            return await paymentGatewayPayPal.GetPaymentUrlAsyncPayPal(inputModel);
        }

        public async Task<string> GetPaymentUrlAsyncStripe(int courseId)
        {
            CourseDetailViewModel viewModel = await GetCourseAsync(courseId);
            CoursePayInputModel inputModel = null!;
            if (httpContextAccessor.HttpContext != null)
            {
                inputModel = new()
                {
                    CourseId = courseId,
                    UserId = httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Description = viewModel.Title,
                    Price = viewModel.CurrentPrice,
                    ReturnUrl = linkGenerator.GetUriByAction(httpContextAccessor.HttpContext, action:nameof(CoursesController.SubscribeStripe),
                        controller:"Courses",
                        values:new{id=courseId}),
                    CancelUrl = linkGenerator.GetUriByAction(httpContextAccessor.HttpContext, action:nameof(CoursesController.Detail),
                        controller:"Courses",
                        values:new{id=courseId})
                };
            }

            Debug.Assert(inputModel != null, nameof(inputModel) + " != null");
            return await paymentGatewayStripe.GetPaymentUrlAsyncStripe(inputModel);
        }

        public Task<CourseSubscribeInputModel> CapturePaymentAsync(int id, string token)
        {
            return paymentGatewayStripe.CapturePaymentAsyncStripe(token);
        }

        public async Task<int?> GetCourseVoteAsync(int id)
        {
            string userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            string vote = await db.QueryScalarAsync<string>($"SELECT Vote FROM Subscriptions WHERE CourseId={id} AND UserId={userId}");
            return string.IsNullOrEmpty(vote) ? null : Convert.ToInt32(vote);
        }

        public async Task VoteCourseAsync(CourseVoteInputModel inputModel)
        {
            if (inputModel.Vote < 1 || inputModel.Vote > 5)
            {
                throw new InvalidVoteException(inputModel.Vote);
            }

            string? userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            int updatedRows = await db.CommandAsync($"UPDATE Subscriptions SET Vote={inputModel.Vote} WHERE CourseId={inputModel.Id} AND UserId={userId}");
            if (updatedRows == 0)
            {
                throw new CourseSubscriptionNotFoundException(inputModel.Id);
            }
        }
    }
}