using System.ComponentModel.DataAnnotations;

namespace Account.API.Models.Requests
{
    public class AccountRequest
    {
        //[Required(AllowEmptyStrings = true)]
        //public string User_id { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
        public BalanceModel Balance { get; set; }
    }
}
