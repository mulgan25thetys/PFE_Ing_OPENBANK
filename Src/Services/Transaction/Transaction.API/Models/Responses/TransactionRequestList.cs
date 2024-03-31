namespace Transaction.API.Models.Responses
{
    public class TransactionRequestList
    {
        public IList<TransactionRequest> Transaction_requests_with_charges { get; set; } = new List<TransactionRequest>();
        public bool HasMore { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Count { get; set; }
    }
}
