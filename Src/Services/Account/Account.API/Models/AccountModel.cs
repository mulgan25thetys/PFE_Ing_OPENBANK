using Account.API.Models.Responses;

namespace Account.API.Models
{
    public class AccountModel
    {
        public string ID { get; set; }
        public string LABEL { get; set; }
        public long NUMBER { get; set; }
        public string TYPE { get; set; }
        public string CURRENCY { get; set; }
        public string AMOUNT { get; set; }
        public string IBAN { get; set; }
        public double SWIFT_BIC { get; set; }
        public string BANK_ID { get; set; }
        public string ROUTING_SCHEME { get; set; }
        public string ROUTING_ADDRESS { get; set; }
        public DateTime CREATEDAT { get; set; }
        public DateTime UPDATEDAT { get; set; }
        public string OWNER_ID { get; set; }
    }
}
