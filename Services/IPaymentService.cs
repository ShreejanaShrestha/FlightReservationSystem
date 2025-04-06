using FlightReservationSystem.DTOs;

namespace FlightReservationSystem.Services
{
    public interface IPaymentService
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentDTO payment);
        Task<PaymentResult> ProcessRefundAsync(string transactionId, decimal amount);
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
