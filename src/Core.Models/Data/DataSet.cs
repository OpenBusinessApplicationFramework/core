using Core.Models.Common;
using System.Collections.ObjectModel;

namespace Core.Models.Data;

public class DataSet : CommonDefinition
{
    public string? Description { get; set; }
    public Collection<DataEntry> DataEntries { get; set; } = new();
}