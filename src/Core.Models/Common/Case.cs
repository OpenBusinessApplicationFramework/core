using Core.Models.Action;
using Core.Models.Data;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models.Common;

public class Case
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public string MathJsUri { get; set; } = "https://cdn.jsdelivr.net/npm/mathjs@14.4.0/lib/browser/math.min.js";

    public Collection<DataDefinition> DataDefinitions { get; set; } = new();
    public Collection<ActionDefinition> ActionDefinitions { get; set; } = new();
}
