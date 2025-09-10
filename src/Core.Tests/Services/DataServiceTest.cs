using Core.Models.Common;
using Core.Models.Data;
using Core.Services.Action;
using Core.Services.Data;
using Core.Tests.Common;
using IdGen;
using System.Collections.ObjectModel;

namespace Core.Tests.Services;

public class DataServiceTest
{
    [Fact]
    public async Task GetDataEntries_ReturnsSeededEntries_Case1()
    {
        var (context, factory) = TestUtilities.CreateContext();
        var actionService = new ActionService(factory);
        var dataAnnotationService = new DataAnnotationService(factory);
        var dataService = new DataService(factory, dataAnnotationService, actionService);

        var entries = await dataService.GetDataEntriesAsync(context, "Case1");

        Assert.Equal(2, entries.count);
        Assert.Contains(entries.results, e => e.DataDefinition.Name == "Name");
        Assert.Contains(entries.results, e => e.DataDefinition.Name == "Email_Verified");
    }

    [Fact]
    public async Task GetDataEntries_ReturnsSeededEntries_Case2()
    {
        var (context, factory) = TestUtilities.CreateContext();
        var actionService = new ActionService(factory);
        var dataAnnotationService = new DataAnnotationService(factory);
        var dataService = new DataService(factory, dataAnnotationService, actionService);

        var entry = await dataService.GetDataEntriesAsync(context, "Case2");

        Assert.Single(entry.results);
        Assert.Contains(entry.results, e => e.DataDefinition.Name == "Name");
    }

    [Fact]
    public async Task GetDataEntries_CheckLinked()
    {
        var (context, factory) = TestUtilities.CreateContext();
        var actionService = new ActionService(factory);
        var dataAnnotationService = new DataAnnotationService(factory);
        var dataService = new DataService(factory, dataAnnotationService, actionService);

        var entries = await dataService.GetDataEntriesAsync(context, "Case2", "Name", ["Customers_Customer1"]);

        Assert.Equal("Test Person", entries.results.Single().Value);
    }

    [Fact]
    public async Task GetDataEntries_CheckBackLinked()
    {
        var (context, factory) = TestUtilities.CreateContext();
        var actionService = new ActionService(factory);
        var dataAnnotationService = new DataAnnotationService(factory);
        var dataService = new DataService(factory, dataAnnotationService, actionService);

        var entryCase2 = await dataService.GetDataEntriesAsync(context, "Case2", "Name", ["Customers_Customer1"]);

        await dataService.UpdateDataEntryAsync("Case2", entryCase2.results.Single().Id, ["Test Person Updated"]);

        var entryCase1 = await dataService.GetDataEntriesAsync(context, "Case2", "Name", ["Customers_Customer1"]);

        Assert.Single(entryCase1.results);
        Assert.Single(entryCase2.results);
        Assert.Equal("Test Person Updated", entryCase1.results.Single().Value);
    }

    [Fact]
    public async Task GetDataEntries_CheckSubTags()
    {
        var (context, factory) = TestUtilities.CreateContext();
        var actionService = new ActionService(factory);
        var dataAnnotationService = new DataAnnotationService(factory);
        var dataService = new DataService(factory, dataAnnotationService, actionService);

        var entry = await dataService.GetDataEntriesAsync(context, "Case1", "Name", getSubTagsFromTopTag: "Customers");

        Assert.Single(entry.results);
        Assert.Contains(entry.results, e => e.DataDefinition.Name == "Name");
    }

    [Fact]
    public async Task GetDataEntries_GetDataEntry_CheckCalculated()
    {
        var (context, factory) = TestUtilities.CreateContext();
        var actionService = new ActionService(factory);
        var dataAnnotationService = new DataAnnotationService(factory);
        var dataService = new DataService(factory, dataAnnotationService, actionService);

        await dataService.CreateDataEntryAsync("Case2", "Id_Get", [string.Empty], ["Customers", "Customers_Customer1"]);

        var _ = await dataService.GetDataEntriesAsync(context, "Case2", "Id_Get", ["Customers", "Customers_Customer1"]);
        var entry = await dataService.GetDataEntriesAsync(context, "Case2", "Id_Get", ["Customers", "Customers_Customer1"]);

        Assert.Single(entry.results);
        Assert.Equal("123123", entry.results.Single().Value);
    }

    [Fact]
    public async Task GetDataEntries_PostDataEntry_CheckCalculated()
    {
        var (context, factory) = TestUtilities.CreateContext();
        var actionService = new ActionService(factory);
        var dataAnnotationService = new DataAnnotationService(factory);
        var dataService = new DataService(factory, dataAnnotationService, actionService);

        await dataService.CreateDataEntryAsync("Case2", "Id_Post", [string.Empty], ["Customers", "Customers_Customer1"]);

        var entry = await dataService.GetDataEntriesAsync(context, "Case2", "Id_Post", ["Customers", "Customers_Customer1"]);

        Assert.Single(entry.results);
        Assert.Equal("123", entry.results.Single().Value);
    }

    [Fact]
    public async Task CreateDataEntryAsync_CheckAutoIncrease()
    {
        var (context, factory) = TestUtilities.CreateContext();
        var actionService = new ActionService(factory);
        var dataAnnotationService = new DataAnnotationService(factory);
        var dataService = new DataService(factory, dataAnnotationService, actionService);

        await dataService.CreateDataEntryAsync("Case1", "Id", null, new List<string>() { { "Customers" }, { "Customers_Customer1" } });
        await dataService.CreateDataEntryAsync("Case1", "Id", null, new List<string>() { { "Customers" }, { "Customers_Customer2" } });

        var entry1 = await dataService.GetDataEntriesAsync(context, "Case1", "Id", ["Customers_Customer1"]);
        var entry2 = await dataService.GetDataEntriesAsync(context, "Case1", "Id", ["Customers_Customer2"]);

        Assert.InRange(entry1.results.SingleOrDefault().Value.Length, 18, 19);
        Assert.Matches(@"^\d+$", entry1.results?.Single()?.Value);

        Assert.InRange(entry2.results.SingleOrDefault().Value.Length, 18, 19);
        Assert.Matches(@"^\d+$", entry2.results?.Single()?.Value);
    }

    [Fact]
    public async Task CreateMultipleEntriesWithTagAndIdAsync_CheckTags()
    {
        var (context, factory) = TestUtilities.CreateContext();
        var actionService = new ActionService(factory);
        var dataAnnotationService = new DataAnnotationService(factory);
        var dataService = new DataService(factory, dataAnnotationService, actionService);

        var newTag = await dataService.CreateDataEntryWithTagAndIdAsync("Case1", "Customers", "Id", null);
        var newTagWithCustomers = await dataService.CreateDataEntryWithTagAndIdAsync("Case1", "Customers", null, ["Customers"]);

        var idEntriesFromCase1 = await dataService.GetDataEntriesAsync(context, "Case1", "Id", [newTag]);
        var idEntriesFromCase1WithCustomerSet = await dataService.GetDataEntriesAsync(context, "Case1", "Id", [newTagWithCustomers]);

        Assert.Equal(2, idEntriesFromCase1.results.Single().Tags.Count);
        Assert.Equal(2, idEntriesFromCase1WithCustomerSet.results.Single().Tags.Count);
    }
}
