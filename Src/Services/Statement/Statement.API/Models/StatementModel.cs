using Statement.API.Models.Responses;

namespace Statement.API.Models
{
    public class StatementModel
    {
        public string Statement_Name { get; set; }
        public Int64 Account_Number { get; set; }
        public string Bank_Name { get; set; }
        public string Branch_Address { get; set; }
        public string Statement_Owner_Name { get; set; }
        public string Statement_Owner_Address { get; set; }
        public DateTime Statement_Period_Start { get; set; }
        public DateTime Statement_Period_End { get; set; }
        public TransactionList Transactions { get; set; }
    }
}
