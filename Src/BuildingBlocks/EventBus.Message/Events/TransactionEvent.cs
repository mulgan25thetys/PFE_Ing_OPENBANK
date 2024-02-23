namespace EventBus.Message.Events
{
    public enum TRANS_STATUS
    {
        SUCCESS, FAILED, WAITING, CANCELED
    }
    public class TransactionEvent : IntegrationBaseEvent
    {
        public Int64 TRANS_ID { get; set; }
        public string TRANS_AUTHOR { get; set; }
        public string TRANS_DESCRIPTION { get; set; }
        public decimal TRANS_AMOUNT { get; set; }
        public string TRANS_STATUS { get; set; }
        public string TRANS_TYPE { get; set; }
        public Int64 TRANS_CREDITED_ACC { get; set; }
        public Int64 TRANS_DEBITED_ACC { get; set; }
        public DateTime TRANS_CREATED_AT { get; set; }
        public DateTime TRANS_UPDATED_AT { get; set; }
    }
}
