using Account.Grpc.Models.Responses;

namespace Account.Grpc.Models
{
    enum AccountStatus
    {
        ENABLED, LOCKED
    }
    public class AccountModel
    {
        public Int64 ACC_ID { get; set; }
        public string ACC_OWNER_FIRSTNAME { get; set; }
        public string ACC_OWNER_LASTNAME { get; set; }
        public string ACC_OWNER_EMAIL { get; set; }
        public string ACC_OWNER_PHONE { get; set; }
        public string ACC_OWNER_ADDRESS { get; set; }
        public int ACC_OWNER_POST_CODE { get; set; }
        public string BANK_NAME { get; set; }
        public int BRANCH_CODE { get; set; }
        public Int64 ACC_NUMBER { get; set; }
        public int ACC_BANK_DETAILS_KEY { get; set; }
        public string ACC_STATUS { get; set; }
        public DateTime ACC_CREATED_AT { get; set; }
        public DateTime ACC_UPDATED_AT { get; set; }
        public Decimal ACC_BALANCE { get; set; }
        public string BANK_CODE { get; set; }
        public string ACC_IBAN { get; set; }
    }
}
