namespace Account.API.Models.Responses
{
    public class AccountResponse
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public long Number { get; set; }
        public IList<AccountOwnerModel> Owners { get; set; } = new List<AccountOwnerModel>();
        public string Type { get; set; }
        public BalanceModel Balance { get; set; }
        public string IBAN { get; set; }
        public string Swift_bic { get; set; }
        public IList<ViewsAvailableModel> Views_available { get; set; } = new List<ViewsAvailableModel>();
    }
}
