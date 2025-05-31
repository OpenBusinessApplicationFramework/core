namespace Core.Models.Data;

public enum ConnectionType
{
    /// <summary>
    /// Readonly only reads the value, must be edited in original link
    /// </summary>
    Readonly,
    /// <summary>
    /// Replicated replicates the value, if written to it the corrisponding link won't change
    /// </summary>
    Replicated,
    /// <summary>
    /// Fulllink fully links the two DataDefinition, any change to any linked values will happen to all of them
    /// </summary>
    Fulllink,
}