namespace User.grpc.Models
{
    public class UserList
    {
        public IList<UserModel> Items { get; set; } = new List<UserModel>();
        public bool HasMore { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Count { get; set; }
        public IList<UserLinks> Links { get; set; } = new List<UserLinks>();
    }
}
