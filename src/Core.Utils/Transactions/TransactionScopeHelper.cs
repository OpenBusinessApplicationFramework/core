using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Core.Utils.Transactions;

public static class TransactionScopeHelper
{
    public static async Task ExecuteInTransactionAsync(TransactionScopeHelperSettings settings, Func<Task> execute)
    {
        using (var scope = new TransactionScope(settings.TransactionScopeOption, new TransactionOptions { IsolationLevel = settings.IsolationLevel, Timeout = settings.Timeout }, settings.TransactionScopeAsyncFlowOption))
        {
            await execute();
            scope.Complete();
        }
    }
}
