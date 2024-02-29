namespace Transaction.API.Models
{
    enum TRANS_STATUS
    {
        SUCCESS,FAILED,WAITING,CANCELED
    }
    enum TRANS_TYPE
    {
        WITHDRAWAL, DEPOSIT, TRANSFERT
    }
    public class TransactionModel
    {
        public Int64 TRANSID { get; set; }
        public string AUTHOR { get; set; }
        public string DESCRIPTION { get; set; }
        public decimal AMOUNT { get; set; }
        public string STATUS { get; set; }
        public string TYPE { get; set; }
        public Int64 CREDITEDACC { get; set; }
        public Int64 DEBITEDACC { get; set; }
        public DateTime CREATEDAT { get; set; }
        public DateTime UPDATEDAT { get; set; }
    }
}
