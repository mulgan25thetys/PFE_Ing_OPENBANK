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
        [RegularExpression("\\(?\\d{3}\\)?[-\\.]? *\\d{3}[-\\.]? *[-\\.]?\\d{4}", ErrorMessage = "Phone number is not correct!")]
        public string Phone { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
