using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCourse.Customizations.Authorization;
using MyCourse.Models.Enums;
using MyCourse.Models.Exceptions.Application;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Application.Courses;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ValueObjects;
using MyCourse.Models.ViewModels;
using MyCourse.Models.ViewModels.Courses;

namespace MyCourse.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ICourseService courseService;
        public CoursesController(ICachedCourseService courseService)
        {
            this.courseService = courseService;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Index(CourseListInputModel input)
        {
            ViewData["Title"] = "Catalogo dei corsi";
            ListViewModel<CourseViewModel> courses = await courseService.GetCoursesAsync(input);

            CourseListViewModel viewModel = new CourseListViewModel
            {
                Courses = courses,
                Input = input
            };

            return View(viewModel);
        }
        public async Task<IActionResult> Detail(int id)
        {
            CourseDetailViewModel viewModel = await courseService.GetCourseAsync(id);
            ViewData["Title"] = viewModel.Title;
            return View(viewModel);
        }
        [AuthorizeRole(Role.Teacher)]
        public IActionResult Create()
        {
            ViewData["Title"] = "Nuovo corso";
            var inputModel = new CourseCreateInputModel();
            return View(inputModel);
        }
        
        [HttpPost]
        [AuthorizeRole(Role.Teacher)]
        [ValidateReCaptcha]
        public async Task<IActionResult> Create(
            CourseCreateInputModel inputModel, 
            [FromServices] IAuthorizationService authorizationService, 
            [FromServices] IOptionsMonitor<UsersOptions> usersOptions,
            [FromServices] IOptionsMonitor<SmtpOptions> smtpOptionsMonitor,
            [FromServices] ILogger<MailKitEmailSender> logger,
            [FromServices] IConfiguration configuration)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    CourseDetailViewModel course = await courseService.CreateCourseAsync(inputModel);

                    // Per non inserire logica nel controller, potremmo spostare questo blocco all'interno del metodo CreateCourseAsync del servizio applicativo
                    // ...ma attenzione a non creare riferimenti circolari! Se il course service dipende da IAuthorizationService
                    // ...e viceversa l'authorization handler dipende dal course service, allora la dependency injection non riuscirà a risolvere nessuno dei due servizi, dandoci un errore
                    AuthorizationResult result = await authorizationService.AuthorizeAsync(User, nameof(Policy.CourseLimit));
                    if (!result.Succeeded)
                    {
                        MailKitEmailSender emailClient = new MailKitEmailSender(smtpOptionsMonitor, logger, configuration);
                        emailClient.MailFrom = "MyCourse@noreply.example.com";
                        await emailClient.SendEmailAsync(usersOptions.CurrentValue.NotificationEmailRecipient, "Avviso superamento soglia", $"Il docente {User.Identity.Name} ha creato molti corsi: verifica che riesca a gestirli tutti.");
                    }
                    TempData["ConfirmationMessage"] = "Ok! Il tuo corso è stato creato, ora perché non inserisci anche gli altri dati?";
                    return RedirectToAction(nameof(Edit), new { id = course.Id });
                }
                catch (CourseTitleUnavailableException)
                {
                    ModelState.AddModelError(nameof(CourseDetailViewModel.Title), "Questo titolo già esiste");
                }
            }

            ViewData["Title"] = "Nuovo corso";
            return View(inputModel);
        }
        [AuthorizeRole(Role.Teacher)]
        [Authorize(Policy = nameof(Policy.CourseAuthor))]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Modifica corso";
            CourseEditInputModel inputModel = await courseService.GetCourseForEditingAsync(id);
            return View(inputModel);
        }
        [AuthorizeRole(Role.Teacher)]
        [Authorize(Policy = nameof(Policy.CourseAuthor))]
        [HttpPost]
        public async Task<IActionResult> Edit(CourseEditInputModel inputModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    CourseDetailViewModel course = await courseService.EditCourseAsync(inputModel);
                    TempData["ConfirmationMessage"] = "I dati sono stati salvati con successo";
                    return RedirectToAction(nameof(Detail), new { id = inputModel.Id });
                }
                catch (CourseImageInvalidException)
                {
                    ModelState.AddModelError(nameof(CourseEditInputModel.Image), "L'immagine selezionata non è valida");
                }
                catch (CourseTitleUnavailableException)
                {
                    ModelState.AddModelError(nameof(CourseEditInputModel.Title), "Questo titolo già esiste");
                }
                catch (OptimisticConcurrencyException)
                {
                    ModelState.AddModelError("", "Spiacenti, il salvataggio non è andato a buon fine perché nel frattempo un altro utente ha aggiornato il corso. Ti preghiamo di aggiornare la pagina e ripetere le modifiche.");
                }
            }

            ViewData["Title"] = "Modifica corso";
            return View(inputModel);
        }
        [AuthorizeRole(Role.Teacher)]
        [Authorize(Policy = nameof(Policy.CourseAuthor))]
        [HttpPost]
        public async Task<IActionResult> Delete(CourseDeleteInputModel inputModel)
        {
            await courseService.DeleteCourseAsync(inputModel);
            TempData["ConfirmationMessage"] = "Il corso è stato eliminato ma potrebbe continuare a comparire negli elenchi per un breve periodo, finché la cache non viene aggiornata.";
            return RedirectToAction(nameof(Index));
        }
        [AuthorizeRole(Role.Teacher)]
        public async Task<IActionResult> IsTitleAvailable(string title, int id = 0)
        {
            bool result = await courseService.IsTitleAvailableAsync(title, id);
            return Json(result);
        }
        public async Task<IActionResult> SubscribePayPal(int id, string token)
        {
            CourseSubscribeInputModel inputModel = await courseService.CapturePaymentAsyncPayPal(id, token);
            await courseService.SubscribeCourseAsync(inputModel);
            TempData["ConfirmationMessage"] = "Grazie per esserti iscritto, guarda subito la prima lezione!";
            return RedirectToAction(nameof(Detail), new { id = id });
        }
        
        public async Task<IActionResult> SubscribeStripe(int id, string token)
        {
            CourseSubscribeInputModel inputModel = await courseService.CapturePaymentAsyncStripe(id, token);
            await courseService.SubscribeCourseAsync(inputModel);
            TempData["ConfirmationMessage"] = "Grazie per esserti iscritto, guarda subito la prima lezione!";
            return RedirectToAction(nameof(Detail), new { id = id });
        }
        
        [HttpPost]
        public async Task<IActionResult> Pay(int id, string flexRadioDefault)
        {
            int payment = int.Parse(flexRadioDefault);
            if (payment.Equals(1))
            {
                string paymentUrl = await courseService.GetPaymentUrlAsyncPayPal(id);
                return Redirect(paymentUrl);
            }
            else
            {
                string paymentUrl = await courseService.GetPaymentUrlAsyncStripe(id);
                return Redirect(paymentUrl);
            }
        }
        [Authorize(Policy = nameof(Policy.CourseSubscriber))]
        public async Task<IActionResult> Vote(int id)
        {
            CourseVoteInputModel inputModel = new()
            {
                Id = id,
                Vote = await courseService.GetCourseVoteAsync(id) ?? 0
            };
            
            return View(inputModel);
        }

        [Authorize(Policy = nameof(Policy.CourseSubscriber))]
        [HttpPost]
        public async Task<IActionResult> Vote(CourseVoteInputModel inputModel)
        {
            await courseService.VoteCourseAsync(inputModel);
            TempData["ConfirmationMessage"] = "Grazie per aver votato!";
            return RedirectToAction(nameof(Detail), new { id = inputModel.Id });
        }

    }
}