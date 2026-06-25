namespace GD1.Application.Common.Interfaces
{
    public interface IPaymentService
    {
        Task<(string orderId, decimal amount)> CreateOrderAsync(string receiptId, decimal amountInInr, decimal adminCutInInr, string lotOwnerAccountId);
        Task<(string orderId, decimal amount)> CreateStandardOrderAsync(string receiptId, decimal amountInInr);
        Task<(bool IsSuccess, string RefundId)> RefundPaymentAsync(string paymentId, decimal amountInInr);
        bool VerifySignature(string orderId, string paymentId, string signature);
    }
}
