using System.Transactions;

namespace Core.Utils.Transactions;

public class TransactionScopeHelperSettings
{
    public TransactionScopeOption TransactionScopeOption { get; set; } = TransactionScopeOption.Required;
    public TransactionScopeAsyncFlowOption TransactionScopeAsyncFlowOption { get; set; } = TransactionScopeAsyncFlowOption.Enabled;

    public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;
    public TimeSpan Timeout { get; set; } = TransactionManager.DefaultTimeout;
}
