using System.ComponentModel.DataAnnotations;

namespace Identity.API.Applications.Dtos
{
    public class RegisterRequest
    {
        [Required]
        public string UserName { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [DataType(DataType.PhoneNumber)]
        [RegularExpression("\\+?[1-9][0-9]{10,16}", ErrorMessage = "Phone number is not correct!")]
        public string Phone { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
