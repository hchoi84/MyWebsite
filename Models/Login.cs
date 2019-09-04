using System.ComponentModel.DataAnnotations;

namespace MyWebsite.Models
{
  public class Login
  {
    [Required(ErrorMessage="Required")]
    [Display(Name="Email")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [Required(ErrorMessage="Required")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
  }
}