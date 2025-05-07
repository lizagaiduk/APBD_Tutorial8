using System.ComponentModel.DataAnnotations;

namespace Tutorial8.Models.DTOs;

public class CreateClientDTO
{
    [Required(ErrorMessage = "FirstName is required")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "LastName is required")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }

    [Phone(ErrorMessage = "Invalid telephone format")]
    public string Telephone { get; set; }

    [RegularExpression(@"^\d{11}$", ErrorMessage = "PESEL must be exactly 11 digits")]
    public string Pesel { get; set; }
}