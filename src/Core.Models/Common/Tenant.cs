using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Core.Models.Common;

public class Tenant
{
    [Key]
    public long Id { get; set; }
    public string Name { get; set; }

    public Collection<Case> Cases { get; set; } = new();
}
