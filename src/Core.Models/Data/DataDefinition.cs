using Core.Models.Common;
using System.Collections.ObjectModel;

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

    public string? AutoIncreaseAtTag { get; set; }

    public bool IsValid =>
        (ValueType == ValueType.Static
            && string.IsNullOrWhiteSpace(ActionForCalculated)
            && string.IsNullOrWhiteSpace(PathForConnected))
            && string.IsNullOrWhiteSpace(AutoIncreaseAtTag)
        || (ValueType == ValueType.Calculated
            && CalculateType != null
            && !string.IsNullOrWhiteSpace(ActionForCalculated)
            && string.IsNullOrWhiteSpace(PathForConnected))
            && string.IsNullOrWhiteSpace(AutoIncreaseAtTag)
        || (ValueType == ValueType.Connected
            && ConnectionType != null
            && !string.IsNullOrWhiteSpace(PathForConnected)
            && string.IsNullOrWhiteSpace(ActionForCalculated))
            && string.IsNullOrWhiteSpace(AutoIncreaseAtTag)
        || (ValueType == ValueType.AutoIncrease
            && string.IsNullOrWhiteSpace(PathForConnected)
            && string.IsNullOrWhiteSpace(ActionForCalculated))
            && !string.IsNullOrWhiteSpace(AutoIncreaseAtTag);
}
