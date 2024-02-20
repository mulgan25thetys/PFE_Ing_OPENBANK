using Account.API.Models.Responses;

namespace Account.API.Models
{
    public class AccountModel
    {
        public Int64 Acc_Id { get; set; }
        public string Acc_Owner_Firstname { get; set; }
        public string Acc_Owner_Lastname { get; set; }
        public string Acc_Owner_Email { get; set; }
        public string Acc_Owner_Phone { get; set; }
        public string Acc_Owner_Address { get; set; }
        public int Acc_Owner_Post_Code { get; set; }
        public string Bank_Name { get; set; }
        public int Bank_Code { get; set; }
        public int Branch_Code { get; set; }
        public Int64 Acc_Number { get; set; }
        public int Acc_Bank_Details_Key { get; set; }
        public string Acc_IBAN { get; set; }
        public int Acc_Code_BIC { get; set; }
        public Decimal Acc_Balance { get; set; }
        public DateTime Acc_Created_At { get; set; }
        public DateTime Acc_Updated_At { get; set; }
        public IList<AccountLinks> Links { get; set; }
    }
}
