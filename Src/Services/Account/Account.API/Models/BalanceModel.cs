using System.ComponentModel.DataAnnotations;

namespace Account.API.Models
{
    public class BalanceModel
    {
        public string Currency { get; set; }
        public double Amount { get; set; }
    }
}
