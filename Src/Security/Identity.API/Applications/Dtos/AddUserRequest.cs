using System.ComponentModel.DataAnnotations;

namespace Identity.API.Applications.Dtos
{
    public enum UserRole
    {
        ADMIN,PROVIDER
    }
    public class AddUserRequest
    {
        public UserRole Role { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
