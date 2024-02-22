using Account.API.Models.Responses;

namespace Account.API.Models
{
    enum AccountStatus
    {
        ENABLED, LOCKED
    }
    public class AccountModel
    {
        public Int64 ACCNUMBER { get; set; }
        public string ACCOWNERFIRSTNAME { get; set; }
        public string ACCOWNERLASTNAME { get; set; }
        public string ACCOWNEREMAIL { get; set; }
        public string ACCOWNERPHONE { get; set; }
        public string ACCOWNERADDRESS { get; set; }
        public int ACCOWNERPOSTCODE { get; set; }
        public string BANKNAME { get; set; }
        public string BANKCODE { get; set; }
        public int BRANCHCODE { get; set; }
        public int ACCBANKDETAILSKEY { get; set; }
        public string ACCIBAN { get; set; }
        public double ACCBALANCE { get; set; }
        public DateTime ACCCREATEDAT { get; set; }
        public DateTime ACCUPDATEDAT { get; set; }
        public string ACCSTATUS { get; set; }
    }
}
