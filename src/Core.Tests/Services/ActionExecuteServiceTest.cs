using Core.Services.Action;
using Core.Services.Data;
using Core.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tests.Services;

public class ActionExecuteServiceTest
{
    [Fact]
    public async Task ActionExecute_ExecuteAction()
    {
        var (context, factory) = TestUtilities.CreateContext();
        var actionService = new ActionService(factory);
        var dataAnnotationService = new DataAnnotationService(factory);
        var dataService = new DataService(factory, dataAnnotationService, actionService);
        var actionExecuteService = new ActionExecuteService(factory, dataService, dataAnnotationService);

        await actionExecuteService.ExecuteActionAsync("Case2", "Customer1_namechange", null);

        var entry = await dataService.GetDataEntriesAsync(context, "Case2", "Name", ["Customers", "Customers_Customer1"]);

        Assert.Single(entry.results);
        Assert.Equal("Customer 1 newname", entry.results.Single().Value);
    }
}
