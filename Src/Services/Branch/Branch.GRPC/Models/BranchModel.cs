
namespace Branch.GRPC.Models
{
    public class BranchModel
    {
        #region Fields
        public int IDBRANCH { get; set; }
        public string REGION { get; set; }
        public string NAME { get; set; }
        public string SPECIALISATION { get; set; }
        public string EMAIL { get; set; }
        public string ADDRESS { get; set; }
        public string MANAGER { get; set; }
        public string MANAGER_NET { get; set; }
        public string PHONE { get; set; }
        public string FAX { get; set; }
        public string CODEISIC { get; set; }
        public int STATUS { get; set; }
        #endregion

        #region Constructors
        public BranchModel()
        {
            this.STATUS = 0;
        }
        public BranchModel(string region,string name,string address, string specilisation)
        {
            this.NAME = name;
            this.REGION = region;
            this.ADDRESS = address;
            this.SPECIALISATION = specilisation;
            this.STATUS = 0;
        }
        #endregion
    }
}
