namespace Transaction.API.Models.Responses.Accounts
{
    public class ToAccountDetail
    {
        public string Name { get; set; }
        public string Bank_code { get; set; }
        public long Branch_number { get; set; }
        public AccountDetail Account { get; set; }
    }
}
