using MediatR;

namespace Transaction.API.Features.Commands
{
    public class TransactionCmd : IRequest<bool>
    {
        public Int64 Trans_Id { get; set; }
        public string Trans_Author { get; set; }
        public string Trans_Description { get; set; }
        public decimal Trans_Amount { get; set; }
        public string Trans_Status { get; set; }
        public string Trans_Type { get; set; }
        public Int64 Trans_Credited_Acc { get; set; }
        public Int64 Trans_Debited_Acc { get; set; }
        public DateTime Trans_Created_At { get; set; }
        public DateTime Trans_Updated_At { get; set; }
    }
}
