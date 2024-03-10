using System.ComponentModel.DataAnnotations;

namespace Account.API.Models.Requests
{
    public class AccountRequest
    {
        public string Branch_Name { get; set; }
        public string Acc_Owner_Firstname { get; set; }
        public string Acc_Owner_Lastname { get; set; }
        public string Acc_Owner_Email { get; set; }
        [RegularExpression("\\+?[1-9][0-9]{10,16}", ErrorMessage = "Phone number is not correct!")]
        public string Acc_Owner_Phone { get; set; }
        public string Acc_Owner_Address { get; set; }
        public int Acc_Owner_Post_Code { get; set; }
    }
}
