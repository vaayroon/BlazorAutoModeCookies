using System.ComponentModel.DataAnnotations;

namespace BlazorAutoMode.Client.Models.ViewModels;

public class LoginViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "User name is required.")]
    public string? UserName { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required.")]
    public string? Password { get; set; }
}
