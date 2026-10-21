namespace PmApp.Web.Models.Entities;

/// <summary>Penugasan satu atau beberapa Product kepada akun Line Head.</summary>
public class AppUserProduct
{
    public int UserId { get; set; }
    public AppUser? User { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
}
