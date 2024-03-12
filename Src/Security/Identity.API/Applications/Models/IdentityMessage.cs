namespace Identity.API.Applications.Models
{
    public class IdentityMessage
    {
        public string Destination { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
    }
}
