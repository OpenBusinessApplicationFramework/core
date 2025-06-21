using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models.Common;

public class CommonDefinition : BasicCommonDefinition
{
    public string Name { get; set; }
}

public class BasicCommonDefinition
{
    public long Id { get; set; }

    [ForeignKey(nameof(Case))]
    public long CaseId { get; set; }
    public Case Case { get; set; }
}
