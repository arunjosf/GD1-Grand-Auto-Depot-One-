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
            return Task.FromResult<(string orderId, decimal amount)>((createdOrderId, amountInInr));
        }

        public Task<(string orderId, decimal amount)> CreateStandardOrderAsync(string receiptId, decimal amountInInr)
        {
            if (_client == null)
                throw new System.Exception("Razorpay is not configured.");

            int amountInPaise = (int)(amountInInr * 100);
            Dictionary<string, object> options = new Dictionary<string, object>
            {
                { "amount", amountInPaise },
                { "currency", "INR" },
                { "receipt", receiptId }
            };

            Order order = _client.Order.Create(options);
            return Task.FromResult<(string orderId, decimal amount)>((order["id"].ToString() ?? string.Empty, amountInInr));
        }

        public Task<(bool IsSuccess, string RefundId)> RefundPaymentAsync(string paymentId, decimal amountInInr)
        {
            if (_client == null)
                throw new System.Exception("Razorpay is not configured.");

            try
            {
                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", (int)(amountInInr * 100));
                Payment payment = _client.Payment.Fetch(paymentId);
                Refund refund = payment.Refund(options);
                string refundId = refund?["id"]?.ToString() ?? string.Empty;
                return Task.FromResult((refund != null, refundId));
            }
            catch (Exception ex)
            {
                throw new Exception($"Razorpay Refund Error: {ex.Message}", ex);
            }
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
