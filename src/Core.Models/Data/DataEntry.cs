using Core.Models.Common;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models.Data;

public class DataEntry : BasicCommonDefinition
{

    public ICollection<Tag> Tags { get; set; } = new Collection<Tag>();

    [ForeignKey(nameof(DataDefinition))]
    public long DataDefinitionId { get; set; }
    public DataDefinition DataDefinition { get; set; } = null!;

    private string _separator => Environment.GetEnvironmentVariable("SINGLE_VALUE_SEPARATOR") ?? ";";

    public string? Value
    {
        get => DataDefinition.MultipleValues
                   ? string.Join(_separator, Values)
                   : Values.FirstOrDefault();
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Values = new List<string>();
            }
            else if (DataDefinition.MultipleValues)
            {
                Values = value.Split(_separator).ToList();
            }
            else
            {
                Values = new List<string> { value };
            }
        }
    }

    public List<string> Values { get; set; } = new();

    public bool IsValid => (Tags != null && Tags.Any());
}