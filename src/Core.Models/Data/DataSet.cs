using Core.Models.Common;

namespace Core.Models.Data;

public class DataSet : CommonDefinition
{
    public string? Description { get; set; }
    public ICollection<DataEntry> DataEntries { get; set; } = new List<DataEntry>();
}