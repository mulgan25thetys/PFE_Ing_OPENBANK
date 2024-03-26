namespace User.grpc.Models
{
    public class UserModel
    {
        public string Id { get; set; }
        public string First_name { get; set; }
        public string Last_name { get; set; }
        public string Provider_id { get; set; }
        public string Provider { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
