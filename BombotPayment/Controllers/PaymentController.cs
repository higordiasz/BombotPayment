using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Coinbase;
using Coinbase.Commerce;
using Coinbase.Commerce.Models;
using WebhookHelper = Coinbase.Commerce.WebhookHelper;
using HeaderNames = Coinbase.Commerce.HeaderNames;
using System.IO;
using Newtonsoft.Json;
using BombotPayment.Services;
using BombotPayment.Models;

namespace BombotPayment.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        [Route("create")]
        public async Task<ActionResult> CreatePament ([FromQuery(Name = "name")] string name = "", [FromQuery(Name = "description")] string description = "", [FromQuery(Name = "value")] double value = 1.0, [FromQuery(Name = "customerId")] string customerId = "")
        {
            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(description) || String.IsNullOrEmpty(customerId))
                return BadRequest(new { erro = "Invalid Querry" });
            var commerceApi = new CommerceApi("");
            var charge = new CreateCharge
            {
                Name = name,
                Description = description,
                PricingType = PricingType.FixedPrice,
                LocalPrice = new Money { Amount = Convert.ToDecimal(value), Currency = "USD" },
                Metadata =
                {
                    {"customerId", customerId},
                    {"name", name },
                    {"value", value },
                    {"description", description }
                },
            };
            var response = await commerceApi.CreateChargeAsync(charge);
            if (response.HasError())
            {
                //Console.WriteLine(response.Error.Message);
                return BadRequest(new { erro = response.Error.Message });
            }

            return Ok(new { url = response.Data.HostedUrl, code = response.Data.Code });
        }

        //[RequireHttps]
        [Route("callbackpayment")]
        public async Task<ActionResult> CallbackPayments ()
        {
            try
            {
                var requestSignature = Request.Headers[HeaderNames.WebhookSignature];
                var json = await new StreamReader(Request.Body).ReadToEndAsync();

                if (!WebhookHelper.IsValid("", requestSignature, json))
                {
                    return BadRequest(new { erro = "Bad Request" });
                }

                var webhook = JsonConvert.DeserializeObject<Webhook>(json);

                if (webhook.Event.IsChargeCreated)
                {
                    var charge = webhook.Event.DataAs<Charge>();
                    var check = _paymentService.GetByCode(charge.Code);
                    Models.Payment p;
                    if (check == null)
                    {
                        p = new Models.Payment
                        {
                            Code = charge.Code,
                            CustomerId = charge.Metadata["customerId"].ToObject<string>(),
                            Description = charge.Metadata["description"].ToObject<string>(),
                            Name = charge.Metadata["name"].ToObject<string>(),
                            PaymentStatus = "create",
                            Value = charge.Metadata["value"].ToObject<double>()
                        };
                        _paymentService.Create(p);
                    }
                    else
                    {
                        check.PaymentStatus = "create";
                        _paymentService.UpdateByCode(charge.Code, check);
                    }
                }
                else
                {
                    if (webhook.Event.IsChargePending)
                    {
                        var charge = webhook.Event.DataAs<Charge>();
                        var check = _paymentService.GetByCode(charge.Code);
                        if (check != null)
                        {
                            check.PaymentStatus = "pending";
                            _paymentService.UpdateByCode(charge.Code, check);
                        }
                    }
                    else
                    {
                        if (webhook.Event.IsChargeFailed)
                        {
                            var charge = webhook.Event.DataAs<Charge>();
                            var check = _paymentService.GetByCode(charge.Code);
                            if (check != null)
                            {
                                check.PaymentStatus = "failed";
                                _paymentService.UpdateByCode(charge.Code, check);
                            }
                        }
                        else
                        {
                            if (webhook.Event.IsChargeDelayed)
                            {
                                var charge = webhook.Event.DataAs<Charge>();
                                var check = _paymentService.GetByCode(charge.Code);
                                if (check != null)
                                {
                                    check.PaymentStatus = "delayed";
                                    _paymentService.UpdateByCode(charge.Code, check);
                                }
                            }
                            else
                            {
                                if (webhook.Event.IsChargeResolved)
                                {
                                    var charge = webhook.Event.DataAs<Charge>();
                                    var check = _paymentService.GetByCode(charge.Code);
                                    if (check != null)
                                    {
                                        check.PaymentStatus = "resolved";
                                        _paymentService.UpdateByCode(charge.Code, check);
                                    }
                                }
                                else
                                {
                                    if (webhook.Event.IsChargeConfirmed)
                                    {
                                        var charge = webhook.Event.DataAs<Charge>();
                                        var check = _paymentService.GetByCode(charge.Code);
                                        if (check != null)
                                        {
                                            check.PaymentStatus = "confirmed";
                                            _paymentService.UpdateByCode(charge.Code, check);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return Ok(new { message = "Ok" });
            } catch (Exception err)
            {
                return BadRequest(new { erro = err.Message });
            }
        }

        [HttpGet]
        [Route("checkpayment")]
        public ActionResult CheckPayment([FromQuery(Name = "code")]string code)
        {
            if (String.IsNullOrEmpty(code))
                return BadRequest(new { error = "Bad request"});
            Models.Payment p = _paymentService.GetByCode(code);
            if (p != null)
                return Ok(new { status = p.PaymentStatus, customerID = p.CustomerId, value = p.Value, name = p.Name, description = p.Description, code = p.Code });
            return Ok(new { message = "Don't have payment with this code" });
        }

    }
}
