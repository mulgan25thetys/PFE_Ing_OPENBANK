using Bank.api.Models.Responses;
using System.ComponentModel.DataAnnotations;

namespace Bank.api.Models.Requests
{
    public class BankRequest
    {
        public string Id { get; set; }
        public string Short_name { get; set; }
        public string Full_name { get; set; }
        [DataType(DataType.Url)]
        public string Logo { get; set; }
        [DataType(DataType.Url)]
        public string Website { get; set; }
        public BankRouting Bank_routing { get; set; }
    }
}
