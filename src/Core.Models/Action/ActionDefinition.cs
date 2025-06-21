using Core.Models.Common;

namespace Core.Models.Action;

public class ActionDefinition : CommonDefinition
{
    public string ActionFunction { get; set; } = string.Empty;

    public IList<string> TagUsedInAction { get; set; } = new List<string>();

    public Case Case { get; set; } = null!;
}
