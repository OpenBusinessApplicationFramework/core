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

    public string? DefaultIdentifierDefinition { get; set; }

    public List<string>? AllowedDataDefinitions { get; set; }
    public List<string>? AllowedActions { get; set; }

    public List<string>? AllowedSubDataDefinitions { get; set; }
    public List<string>? AllowedSubActions { get; set; }

    public Collection<DataEntry> DataEntries { get; set; } = new();
}
