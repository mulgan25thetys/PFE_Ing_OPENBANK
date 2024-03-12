namespace Identity.API.Applications.Dtos
{
    public class VerifyPhoneNumberRequest
    {
        public string PhoneNumber { get; set; }
        public string Code { get; set; }
    }
}
