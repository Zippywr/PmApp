using System.ComponentModel.DataAnnotations;

namespace PmApp.Web.Models.Entities;

public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }

    [MaxLength(100)]
    public string CreatedBy { get; set; } = "system";

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool IsDeleted { get; set; } = false;

    [MaxLength(100)]
    public string? DeletedBy { get; set; }

    public DateTime? DeletedDate { get; set; }
}