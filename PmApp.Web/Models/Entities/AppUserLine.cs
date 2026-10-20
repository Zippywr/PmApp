namespace PmApp.Web.Models.Entities;

// Penugasan dapat mencakup lebih dari satu Line per akun Line Head.
public class AppUserLine
{
    public int UserId { get; set; }
    public AppUser? User { get; set; }
    public int LineId { get; set; }
    public Line? Line { get; set; }
}
