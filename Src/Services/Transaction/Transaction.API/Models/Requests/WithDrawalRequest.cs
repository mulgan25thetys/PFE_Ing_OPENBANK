namespace Transaction.API.Models.Requests
{
    public class WithDrawalRequest
    {
        public string Trans_Author { get; set; }
        public string Trans_Description { get; set; }
        public decimal Trans_Amount { get; set; }
        public Int64 Trans_Debited_Acc { get; set; }
    }
}
