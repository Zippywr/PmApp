using PmApp.Web.Models.Entities;

namespace PmApp.Web.Models.ViewModels;

public class PmMachineGroupViewModel
{
    public Asset Asset { get; set; } = null!;
    public List<PmTaskTemplate> Tasks { get; set; } = new();
}