using System.ComponentModel.DataAnnotations;

namespace Branch.API.Models.Requests
{
    public class BranchRequest
    {
        public int CODE { get; set; }
        public string REGION { get; set; }
        public string NAME { get; set; }
        public string SPECIALISATION { get; set; }
        public string EMAIL { get; set; }
        public string ADDRESS { get; set; }
        public string MANAGER { get; set; }
        public string MANAGERNET { get; set; }
        public string PHONE { get; set; }
        public string FAX { get; set; }
    }
}
