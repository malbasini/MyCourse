using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyCourse.Models.Enums;
using MyCourse.Models.Exceptions.Infrastructure;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Options;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;

namespace MyCourse.Models.Services.Infrastructure
{
    public class PaypalPaymentGateway : IPaymentGateway
    {
        private readonly IOptionsMonitor<PaypalOptions> options;

        public PaypalPaymentGateway(IOptionsMonitor<PaypalOptions> options)
        {
            this.options = options;
        }
        /*
         * La configurazione del codice prevede l'implementazione di un gateway di pagamento
         * tramite PayPal e ASP.NET Core. La parte più significativa è il metodo
         * "GetPaymentUrlAsync", che fa parte dell'interfaccia "IPaymentGateway" ed è
         * implementato all'interno della classe "PaypalPaymentGateway".
         * Questo è un metodo asincrono che accetta un parametro, un'istanza della classe
         * "CoursePayInputModel" che rappresenta i dettagli necessari per elaborare un
         * pagamento per un corso. Restituisce un `Task<string>` che rappresenta un'operazione
         * asincrona che produce una stringa URL (URL di pagamento).
           Il metodo sta creando un oggetto `OrderRequest` che rappresenta un ordine PayPal:
           Successivamente, effettua una chiamata API al servizio PayPal:
           
           HttpResponse response = await client.Execute(request);
           
           Questa riga estrae i dettagli dell'ordine dalla risposta. Il metodo trova quindi il 
           collegamento di approvazione:
           
           LinkDescription link = result.Links.Single(link => link.Rel == "approve");
           
           Infine restituisce l'attributo Href del collegamento, ovvero l'URL a cui l'utente deve essere 
           reindirizzato per procedere con il pagamento:
         */
        public async Task<string> GetPaymentUrlAsync(CoursePayInputModel inputModel)
        {
            OrderRequest order = new()
            {
                CheckoutPaymentIntent = "CAPTURE",
                ApplicationContext = new ApplicationContext()
                {
                    ReturnUrl = inputModel.ReturnUrl,
                    CancelUrl = inputModel.CancelUrl,
                    BrandName = options.CurrentValue.BrandName,
                    ShippingPreference = "NO_SHIPPING"
                },
                PurchaseUnits = new List<PurchaseUnitRequest>()
                {
                    new PurchaseUnitRequest()
                    {
                        CustomId = $"{inputModel.CourseId}/{inputModel.UserId}",
                        Description = inputModel.Description,
                        AmountWithBreakdown = new AmountWithBreakdown()
                        {
                            CurrencyCode = inputModel.Price.Currency.ToString(),
                            Value = inputModel.Price.Amount.ToString(CultureInfo.InvariantCulture) // 14.50
                        }
                    }
                }
            };

            PayPalEnvironment env = GetPayPalEnvironment(options.CurrentValue);
            PayPalHttpClient client = new PayPalHttpClient(env);

            OrdersCreateRequest request = new();
            request.RequestBody(order);
            request.Prefer("return=representation");

            HttpResponse response = await client.Execute(request);
            Order result = response.Result<Order>();

            LinkDescription link = result.Links.Single(link => link.Rel == "approve");
            return link.Href;
        }
        /*
         * CapturePaymentAsync" è un metodo "asincrono" della classe "PaypalPaymentGateway".
         * Lo scopo di questa classe è interagire con l'API PayPal e implementa l'interfaccia
         * "IPaymentGateway". Questo metodo restituisce "Task<CourseSubscribeInputModel>",
         * indicando che si tratta di un metodo asincrono, ovvero progettato per eseguire
         * operazioni senza bloccare il thread chiamante.
           
           Quando viene chiamato questo metodo, verrà avviato un processo di pagamento PayPal 
           utilizzando l'SDK PayPal per inviare una richiesta di acquisizione. 
           La richiesta accetta una stringa "token" che identifica potenzialmente il pagamento 
           da acquisire.
           
           Una volta eseguita con successo questa richiesta, ottiene un oggetto PurchaseUnit 
           dalla prima unità dell'ordine di pagamento e il corrispondente oggetto Capture. 
           Da lì, analizza le informazioni necessarie del pagamento, come courseId, userId, 
           importo pagato, TransactionId, paymentDate e dichiara che `PaymentType` è "Paypal".
           
           Alla fine restituisce un'istanza `CourseSubscribeInputModel` popolata con questi 
           dettagli. Se l'elaborazione fallisce in qualsiasi momento 
           (ad esempio, `client.Execute(request)` potrebbe fallire), 
           viene lanciato un oggetto di tipo "PaymentGatewayException".
         */
        public async Task<CourseSubscribeInputModel> CapturePaymentAsync(string token)
        {
            PayPalEnvironment env = GetPayPalEnvironment(options.CurrentValue);
            PayPalHttpClient client = new PayPalHttpClient(env);

            OrdersCaptureRequest request = new(token);
            request.RequestBody(new OrderActionRequest());
            request.Prefer("return=representation");

            try
            {
                HttpResponse response = await client.Execute(request);
                Order result = response.Result<Order>();

                PurchaseUnit purchaseUnit = result.PurchaseUnits.First();
                Capture capture = purchaseUnit.Payments.Captures.First();

                // $"{inputModel.CourseId}/{inputModel.UserId}"
                string[] customIdParts = purchaseUnit.CustomId.Split('/');
                int courseId = int.Parse(customIdParts[0]);
                string userId = customIdParts[1];

                return new CourseSubscribeInputModel
                {
                    CourseId = courseId,
                    UserId = userId,
                    Paid = new(Enum.Parse<Currency>(capture.Amount.CurrencyCode), decimal.Parse(capture.Amount.Value, CultureInfo.InvariantCulture)),
                    TransactionId = capture.Id,
                    PaymentDate = DateTime.Parse(capture.CreateTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                    PaymentType = "Paypal"
                };

            }
            catch (Exception exc)
            {
                throw new PaymentGatewayException(exc);
            }
        }

        private PayPalEnvironment GetPayPalEnvironment(PaypalOptions options)
        {
            string clientId = options.ClientId;
            string clientSecret = options.ClientSecret;

            return options.IsSandbox ? new SandboxEnvironment(clientId, clientSecret) : new LiveEnvironment(clientId, clientSecret);
        }
    }
}