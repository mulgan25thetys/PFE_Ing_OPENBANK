namespace Branch.API.Models.Response
{
    public class BranchListResponse
    {
        public IList<BranchResponse> Items { get; set; } = new List<BranchResponse>();
        public bool HasMore { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Count { get; set; }
    }
}
