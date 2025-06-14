using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Ganss.Xss;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCourse.Controllers;
using MyCourse.Models.Entities;
using MyCourse.Models.Enums;
using MyCourse.Models.Exceptions.Application;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels;
using MyCourse.Models.ViewModels.Courses;

namespace MyCourse.Models.Services.Application.Courses
{
    public class EfCoreCourseService : ICourseService
    {
        private readonly ILogger<EfCoreCourseService> logger;
        private readonly MyCourseDbContext dbContext;
        private readonly IOptionsMonitor<CoursesOptions> coursesOptions;
        private readonly IImagePersister imagePersister;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IOptionsMonitor<SmtpOptions> smtp;
        private readonly ILogger<MailKitEmailSender> loggerMailKit;
        private readonly IConfiguration configuration;
        private readonly LinkGenerator linkGenerator;
        private readonly IPaymentGatewayStripe paymentGatewayStripe;
        private readonly IPaymentGatewayPayPal paymentGatewayPayPal;

        public EfCoreCourseService(
            IConfiguration configuration, 
            IOptionsMonitor<SmtpOptions> smtp,
            ILogger<MailKitEmailSender> loggerMailKit, 
            IHttpContextAccessor httpContextAccessor,
            ILogger<EfCoreCourseService> logger, 
            IImagePersister imagePersister, 
            MyCourseDbContext dbContext, 
            IOptionsMonitor<CoursesOptions> coursesOptions,
            IPaymentGatewayStripe paymentGatewayStripe,
            IPaymentGatewayPayPal paymentGatewayPayPal,
            LinkGenerator linkGenerator)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.imagePersister = imagePersister;
            this.coursesOptions = coursesOptions;
            this.logger = logger;
            this.dbContext = dbContext;
            this.smtp = smtp;
            this.loggerMailKit = loggerMailKit;
            this.configuration = configuration;
            this.paymentGatewayStripe = paymentGatewayStripe;
            this.paymentGatewayPayPal = paymentGatewayPayPal;
            this.linkGenerator = linkGenerator;
        }
        public async Task<CourseDetailViewModel> GetCourseAsync(int id)
        {
            IQueryable<CourseDetailViewModel> queryLinq = dbContext.Courses
                .AsNoTracking()
                .Include(course => course.Lessons)
                .Where(course => course.Id == id)
                .Select(course => CourseDetailViewModel.FromEntity(course)); //Usando metodi statici come FromEntity, la query potrebbe essere inefficiente. Mantenere il mapping nella lambda oppure usare un extension method personalizzato

            CourseDetailViewModel viewModel = await queryLinq.FirstOrDefaultAsync();
            //.FirstOrDefaultAsync(); //Restituisce null se l'elenco è vuoto e non solleva mai un'eccezione
            //.SingleOrDefaultAsync(); //Tollera il fatto che l'elenco sia vuoto e in quel caso restituisce null, oppure se l'elenco contiene più di 1 elemento, solleva un'eccezione
            //.FirstAsync(); //Restituisce il primo elemento, ma se l'elenco è vuoto solleva un'eccezione
            //.SingleAsync(); //Restituisce il primo elemento, ma se l'elenco è vuoto o contiene più di un elemento, solleva un'eccezione

            if (viewModel == null)
            {
                logger.LogWarning("Course {id} not found", id);
                throw new CourseNotFoundException(id);
            }

            return viewModel;
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
        public async Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model)
        {
            IQueryable<Course> baseQuery = dbContext.Courses;

            baseQuery = (model.OrderBy, model.Ascending) switch
            {
                ("Title", true) => baseQuery.OrderBy(course => course.Title),
                ("Title", false) => baseQuery.OrderByDescending(course => course.Title),
                ("Rating", true) => baseQuery.OrderBy(course => course.Rating),
                ("Rating", false) => baseQuery.OrderByDescending(course => course.Rating),
                ("CurrentPrice", true) => baseQuery.OrderBy(course => course.CurrentPrice.Amount),
                ("CurrentPrice", false) => baseQuery.OrderByDescending(course => course.CurrentPrice.Amount),
                ("Id", true) => baseQuery.OrderBy(course => course.Id),
                ("Id", false) => baseQuery.OrderByDescending(course => course.Id),
                _ => baseQuery
            };

            IQueryable<Course> queryLinq = baseQuery
                .Where(course => course.Title.Contains(model.Search))
                .AsNoTracking();

            List<CourseViewModel> courses = await queryLinq
                .Skip(model.Offset)
                .Take(model.Limit)
                .Select(course => CourseViewModel.FromEntity(course)) //Usando metodi statici come FromEntity, la query potrebbe essere inefficiente. Mantenere il mapping nella lambda oppure usare un extension method personalizzato
                .ToListAsync(); //La query al database viene inviata qui, quando manifestiamo l'intenzione di voler leggere i risultati

            int totalCount = await queryLinq.CountAsync();

            ListViewModel<CourseViewModel> result = new ListViewModel<CourseViewModel>
            {
                Results = courses,
                TotalCount = totalCount
            };

            return result;
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
            string description = "At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum deleniti atque corrupti quos dolores et quas molestias excepturi sint occaecati cupiditate non provident, similique sunt in culpa qui officia deserunt mollitia animi, id est laborum et dolorum fuga. Et harum quidem rerum facilis est et expedita distinctio.";
            string email = "tutor@example.com";
            var course = new Course(title, author, authorId);
            course.ChangeDescription(description);
            course.ChangeEmail(email);
            dbContext.Add(course);
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exc) when ((exc.InnerException as SqliteException)?.SqliteErrorCode == 19)
            {
                throw new CourseTitleUnavailableException(title, exc);
            }

            return CourseDetailViewModel.FromEntity(course);
        }
        public async Task<CourseDetailViewModel> EditCourseAsync(CourseEditInputModel inputModel)
        {
            Course course = await dbContext.Courses.FindAsync(inputModel.Id);
            
            if (course == null)
            {
                throw new CourseNotFoundException(inputModel.Id);
            }

            course.ChangeTitle(inputModel.Title);
            course.ChangePrices(inputModel.FullPrice, inputModel.CurrentPrice);
            course.ChangeDescription(inputModel.Description);
            course.ChangeEmail(inputModel.Email);

            dbContext.Entry(course).Property(course => course.RowVersion).OriginalValue = inputModel.RowVersion;
            
            if (inputModel.Image != null)
            {
                try {
                    string imagePath = await imagePersister.SaveCourseImageAsync(inputModel.Id, inputModel.Image);
                    course.ChangeImagePath(imagePath);
                }
                catch(Exception exc)
                {
                    throw new CourseImageInvalidException(inputModel.Id, exc);
                }
            }

            //dbContext.Update(course);

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new OptimisticConcurrencyException();
            }
            catch (DbUpdateException exc) when ((exc.InnerException as SqliteException)?.SqliteErrorCode == 19)
            {
                throw new CourseTitleUnavailableException(inputModel.Title, exc);
            }

            return CourseDetailViewModel.FromEntity(course);
        }
        public async Task<bool> IsTitleAvailableAsync(string title, int id)
        {
            //await dbContext.Courses.AnyAsync(course => course.Title == title);
            bool titleExists = await dbContext.Courses.AnyAsync(course => EF.Functions.Like(course.Title, title) && course.Id != id);
            return !titleExists;
        }

        public async Task SendQuestionToCourseAuthorAsync(int courseId, string question)
        {
            // Recupero le informazioni del corso
            Course course = await dbContext.Courses.FindAsync(courseId);

            if (course == null)
            {
                logger.LogWarning("Course {id} not found", courseId);
                throw new CourseNotFoundException(courseId);
            }
            //sanitizer question
            var sanitizer = new HtmlSanitizer();
            question = sanitizer.Sanitize(question);
            
            string courseTitle = course.Title;
            string courseEmail = course.Email;

            // Recupero le informazioni dell'utente che vuole inviare la domanda
            string userFullName;
            string userEmail;

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
                                ti ha inviato la seguente domanda per il tuo corso ""{courseTitle}"".</p>
                                <p>{question}</p>";

            // Invio la domanda
            try
            {
                MailKitEmailSender mailClient = new MailKitEmailSender(smtp, loggerMailKit, configuration);
                mailClient.MailFrom = userEmail;
                await mailClient.SendEmailAsync(courseEmail, subject, message);
            }
            catch
            {
                throw new SendException();
            }
        }
        public async Task<CourseEditInputModel> GetCourseForEditingAsync(int id)
        {
            IQueryable<CourseEditInputModel> queryLinq = dbContext.Courses
                .AsNoTracking()
                .Where(course => course.Id == id)
                .Select(course => CourseEditInputModel.FromEntity(course)); //Usando metodi statici come FromEntity, la query potrebbe essere inefficiente. Mantenere il mapping nella lambda oppure usare un extension method personalizzato

            CourseEditInputModel viewModel = await queryLinq.FirstOrDefaultAsync();

            if (viewModel == null)
            {
                logger.LogWarning("Course {id} not found", id);
                throw new CourseNotFoundException(id);
            }

            return viewModel;
        }

        public async Task DeleteCourseAsync(CourseDeleteInputModel inputModel)
        {
            Course course = await dbContext.Courses.FindAsync(inputModel.Id);
            
            if (course == null)
            {
                throw new CourseNotFoundException(inputModel.Id);
            }

            course.ChangeStatus(CourseStatus.Deleted);
            await dbContext.SaveChangesAsync();
        }
        public Task<string> GetCourseAuthorIdAsync(int courseId)
        {
            return dbContext.Courses
                .Where(course => course.Id == courseId)
                .Select(course => course.AuthorId)
                .FirstOrDefaultAsync();
        }

        public Task<int> GetCourseCountByAuthorIdAsync(string authorId)
        {
            return dbContext.Courses
                .Where(course => course.AuthorId == authorId)
                .CountAsync();
        }

        public async Task SubscribeCourseAsync(CourseSubscribeInputModel inputModel)
        {
            if (inputModel.UserId != null)
            {
                Subscription subscription = new(inputModel.UserId, inputModel.CourseId)
                {
                    PaymentDate = inputModel.PaymentDate,
                    PaymentType = inputModel.PaymentType,
                    Paid = inputModel.Paid,
                    TransactionId = inputModel.TransactionId
                };
            
                dbContext.Subscriptions.Add(subscription);
            }

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new CourseSubscriptionException(inputModel.CourseId);
            }
        }

        public async Task<bool> IsCourseSubscribedAsync(int courseId)
        {
            return await dbContext.Subscriptions.Where(subscription => subscription.CourseId == courseId).AnyAsync();
        }

        public Task<bool> IsCourseSubscribedAsync(int courseId, string userId)
        {
            return dbContext.Subscriptions.Where(subscription => subscription.CourseId==courseId && subscription.UserId==userId).AnyAsync();;
        }

        public Task<bool> IsSubscribedAsync(int courseId)
        {
            return dbContext.Subscriptions.Where(subscription => subscription.CourseId==courseId).AnyAsync();
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
        public async Task<int?> GetCourseVoteAsync(int courseId)
        {
            string userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Subscription subscription = await dbContext.Subscriptions.SingleOrDefaultAsync(subscription => subscription.CourseId == courseId && subscription.UserId == userId);
            if (subscription == null)
            {
                throw new CourseSubscriptionNotFoundException(courseId);
            }

            return subscription.Vote;
        }

        public async Task VoteCourseAsync(CourseVoteInputModel inputModel)
        {
            if (inputModel.Vote < 1 || inputModel.Vote > 5)
            {
                throw new InvalidVoteException(inputModel.Vote);
            }

            string userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Subscription subscription = await dbContext.Subscriptions.SingleOrDefaultAsync(subscription => subscription.CourseId == inputModel.Id && subscription.UserId == userId);
            if (subscription == null)
            {
                throw new CourseSubscriptionNotFoundException(inputModel.Id);
            }

            subscription.Vote = inputModel.Vote;
            await dbContext.SaveChangesAsync();
        }
    }
}