using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Account.API.Models.Requests
{
    public class OtherAccountRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string User_id { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Label { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Type { get; set; }
        public BalanceModel Balance { get; set; }
    }
}
