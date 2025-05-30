using System.ComponentModel.DataAnnotations;
using Queryz.Data.Entities;

namespace Queryz.Models;

public class DataSourceModel
{
    public int Id { get; set; }

    [Required]
    public DataProvider DataProvider { get; set; }

    [Required]
    public string ConnectionDetails { get; set; } // JSON to construct connection string
}