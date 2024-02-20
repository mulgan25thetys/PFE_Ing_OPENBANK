namespace Branch.API.Models.Response
{
    public class BranchList
    {
        public IList<BranchModel> Items { get; set; }
        public bool HasMore { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Count { get; set; }
        public IList<BranchLinks> Links { get; set; }
    }
}
