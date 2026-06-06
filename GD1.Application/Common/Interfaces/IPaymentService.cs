namespace GD1.Application.Common.Interfaces
{
    public interface IPaymentService
    {
        Task<(string orderId, decimal amount)> CreateOrderAsync(string receiptId, decimal amountInInr, decimal adminCutInInr, string lotOwnerAccountId);
        bool VerifySignature(string orderId, string paymentId, string signature);
    }
}
