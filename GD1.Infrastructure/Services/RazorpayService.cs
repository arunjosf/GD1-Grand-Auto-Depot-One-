using GD1.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using Razorpay.Api.Errors;
using System.Collections.Generic;

namespace GD1.Infrastructure.Services
{
    public class RazorpayService : IPaymentService
    {
        private readonly IConfiguration _config;
        private readonly RazorpayClient _client;

        public RazorpayService(IConfiguration config)
        {
            _config = config;
            string keyId = _config["Razorpay:KeyId"] ?? "";
            string keySecret = _config["Razorpay:KeySecret"] ?? "";
            
            // Only initialize if keys are present (prevents crash on startup if missing)
            if (!string.IsNullOrEmpty(keyId) && !string.IsNullOrEmpty(keySecret))
            {
                _client = new RazorpayClient(keyId, keySecret);
            }
        }

        public Task<(string orderId, decimal amount)> CreateOrderAsync(string receiptId, decimal amountInInr, decimal adminCutInInr, string lotOwnerAccountId)
        {
            if (_client == null)
            {
                throw new System.Exception("Razorpay is not configured. Please add keys to appsettings.json.");
            }

            // Razorpay expects amount in paise (1 INR = 100 paise)
            int amountInPaise = (int)(amountInInr * 100);
            
            Dictionary<string, object> options = new Dictionary<string, object>
            {
                { "amount", amountInPaise },
                { "currency", "INR" },
                { "receipt", receiptId }
            };

            // If we have a connected Razorpay Account ID for the Lot Owner, we can use Razorpay Route to split the payment automatically.
            if (!string.IsNullOrEmpty(lotOwnerAccountId))
            {
                int ownerCutInPaise = amountInPaise - (int)(adminCutInInr * 100);
                
                var transfers = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        { "account", lotOwnerAccountId },
                        { "amount", ownerCutInPaise },
                        { "currency", "INR" },
                        { "on_hold", 0 }
                    }
                };
                options.Add("transfers", transfers);
            }

            Order order = _client.Order.Create(options);
            string createdOrderId = order["id"].ToString() ?? string.Empty;
            return Task.FromResult((createdOrderId, amountInInr));
        }

        public bool VerifySignature(string orderId, string paymentId, string signature)
        {
            try
            {
                string secret = _config["Razorpay:KeySecret"] ?? "";
                var attributes = new Dictionary<string, string>
                {
                    { "razorpay_order_id", orderId },
                    { "razorpay_payment_id", paymentId },
                    { "razorpay_signature", signature }
                };
                
                Utils.verifyPaymentSignature(attributes);
                return true;
            }
            catch (SignatureVerificationError)
            {
                return false;
            }
        }
    }
}
