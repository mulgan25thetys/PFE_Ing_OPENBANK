using MediatR;
using Transaction.API.Models;

namespace Transaction.API.Features.Commands
{
    public class TransactionCmd : IRequest<TransactionModel>
    {
        public Int64 TransId { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public Int64 CreditedAcc { get; set; }
        public Int64 DebitedAcc { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
