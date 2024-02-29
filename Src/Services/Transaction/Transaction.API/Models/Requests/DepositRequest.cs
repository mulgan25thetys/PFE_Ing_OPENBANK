using System.ComponentModel.DataAnnotations;

namespace Transaction.API.Models.Requests
{
    public class DepositRequest
    {
        public string Trans_Author { get; set; }
        public string Trans_Description { get; set; }
        [Required]
        public double Trans_Amount { get; set; }
        [Required]
        public Int64 Trans_Credited_Acc { get; set; }
    }
}
