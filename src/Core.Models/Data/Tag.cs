using Core.Models.Common;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Core.Models.Data;

public class Tag : CommonDefinition
{
    [Key]
    public long Id { get; set; }
    public string Description { get; set; }

    public bool UniqueDefinition { get; set; } = false;

    public Collection<DataEntry> DataEntries { get; set; } = new();
}
