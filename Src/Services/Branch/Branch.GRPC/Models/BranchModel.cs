﻿
namespace Branch.GRPC.Models
{
    public class BranchModel
    {
        #region Fields
        public int IDBRANCH { get; set; }
        public int CODE { get; set; }
        public string REGION { get; set; }
        public string NAME { get; set; }
        public string SPECIALISATION { get; set; }
        public string EMAIL { get; set; }
        public string ADDRESS { get; set; }
        public string PHONE { get; set; }
        public string FAX { get; set; }
        public string STATUS { get; set; }
        public string MANAGER { get; set; }
        public string MANAGERNET { get; set; }
        #endregion

    }
}
