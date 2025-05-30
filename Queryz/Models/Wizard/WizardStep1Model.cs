using System.ComponentModel.DataAnnotations;

namespace Queryz.Models;

public class WizardStep1Model
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public int GroupId { get; set; }

    [Required]
    public int DataSourceId { get; set; }

    public bool Enabled { get; set; }

    public bool EmailEnabled { get; set; }
}