
using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Username wajib diisi.")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Password wajib diisi.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}
