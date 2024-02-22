using MediatR;
using Transaction.API.Models;

namespace Transaction.API.Features.Queries
{
    public class GetTransactionCmd : IRequest<TransactionModel>
    {
        public Int64 TransactionId { get; set; }
    }
}
