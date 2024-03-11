namespace Account.Access.API.Models
{
    public enum ACCESS_STATUS
    {
        GRANTED,EXPIRED,REVOKE
    }
    public class AccountAccess
    {
        public int ID { get; set; }
        public long ACCNUMBER { get; set; }
        public string PROVIDERID { get; set; }
        public int DURATION { get; set; }
        public ACCESS_STATUS STATUS { get; set; }
        public DateTime CREATEDAT { get; set; }
        public DateTime UPDATEDAT { get; set; }
    }
}
