using System;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace MyCourse.Models.Services.Infrastructure;


using System.Collections.Generic;
using System.Threading.Tasks;
using MyCourse.Models.Enums;
using MyCourse.Models.Exceptions.Infrastructure;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Options;

public class StripePaymentGateway : IPaymentGatewayStripe
{
    private readonly IOptionsMonitor<StripeOptions> options;

    public StripePaymentGateway(IOptionsMonitor<StripeOptions> options)
    {
        this.options = options;
    }
    
    public async Task<string> GetPaymentUrlAsyncStripe(CoursePayInputModel inputModel)
    {
        SessionCreateOptions sessionCreateOptions = new()
        {
            ClientReferenceId = $"{inputModel.CourseId}/{inputModel.UserId}",
            LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions()
                    {
                        Name = inputModel.Description,
                        Amount = Convert.ToInt64(inputModel.Price.Amount * 100),
                        Currency = inputModel.Price.Currency.ToString(),
                        Quantity = 1
                    }
                },
            Mode = "payment",
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                CaptureMethod = "manual"
            },
            PaymentMethodTypes = new List<string>
                {
                    "card"
                },
            SuccessUrl = inputModel.ReturnUrl + "?token={CHECKOUT_SESSION_ID}",
            CancelUrl = inputModel.CancelUrl
        };

        RequestOptions requestOptions = new()
        {
            ApiKey = options.CurrentValue.PrivateKey
        };

        SessionService sessionService = new();
        Session session = await sessionService.CreateAsync(sessionCreateOptions, requestOptions);
        return session.Url;
    }

    public async Task<CourseSubscribeInputModel> CapturePaymentAsyncStripe(string token)
    {
        try
        {
            RequestOptions requestOptions = new()
            {
                ApiKey = options.CurrentValue.PrivateKey
            };

            SessionService sessionService = new();
            Session session = await sessionService.GetAsync(token, requestOptions: requestOptions);

            PaymentIntentService paymentIntentService = new();
            PaymentIntent paymentIntent = await paymentIntentService.CaptureAsync(session.PaymentIntentId, requestOptions: requestOptions);

            string[] customIdParts = session.ClientReferenceId.Split('/');
            int courseId = int.Parse(customIdParts[0]);
            string userId = customIdParts[1];

            return new CourseSubscribeInputModel
            {
                CourseId = courseId,
                UserId = userId,
                Paid = new(Enum.Parse<Currency>(paymentIntent.Currency, ignoreCase: true), paymentIntent.Amount / 100m),
                TransactionId = paymentIntent.Id,
                PaymentDate = paymentIntent.Created,
                PaymentType = "Stripe"
            };
        }
        catch (Exception exc)
        {
            throw new PaymentGatewayException(exc);
        }
    }
}