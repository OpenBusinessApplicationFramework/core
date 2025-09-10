using Core.Models.Common;

namespace Core.Models.Data;

public class DataDefinition : CommonDefinition
{
    public bool MultipleValues { get; set; } = false;
    public bool ReadOnly { get; set; } = false;

    public string? InitialValue { get; set; }

    public ValueType ValueType { get; set; }

    // Use arithmetic formulas, get other values via tags
    public string? ActionForCalculated { get; set; }
    public CalculateType? CalculateType { get; set; }

    // Import Data from other cases
    public ConnectionType? ConnectionType { get; set; }
    public string? PathForConnected { get; set; }

    public string? TagToSelect { get; set; }

    public string? SubGridTopTag { get; set; }

    public bool IsValid =>
        (ValueType == ValueType.Calculated
            && CalculateType != null
            && !string.IsNullOrWhiteSpace(ActionForCalculated))
        || (ValueType == ValueType.Connected
            && ConnectionType != null
            && !string.IsNullOrWhiteSpace(PathForConnected))
        || (ValueType == ValueType.Select
            && !string.IsNullOrWhiteSpace(TagToSelect))
        || (ValueType == ValueType.SubGrid
            && !string.IsNullOrWhiteSpace(SubGridTopTag))
        || (ValueType == ValueType.Static)
        || (ValueType == ValueType.UniqueIdentifier);
}
