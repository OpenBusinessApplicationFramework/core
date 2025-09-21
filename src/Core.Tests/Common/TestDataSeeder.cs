using Core.Db;
using Core.Models.Action;
using Core.Models.Common;
using Core.Models.Data;
using System.Collections.ObjectModel;

namespace Core.Tests.Common;

public static class TestDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        //Case1
        var case1 = new Case { Name = "Case1", Description = "Test Case" };

        var tagCustomers = new Tag { Name = "Customers", Description = "Collection of customers", DefaultIdentifierDefinition = "Id", Case = case1 };
        var tagCustomer1 = new Tag { Name = "Customers_Customer1", Description = "Customer1", Case = case1, UniqueDefinition = true };
        var tagCustomer2 = new Tag { Name = "Customers_Customer2", Description = "Customer2", Case = case1, UniqueDefinition = true };

        var name = new DataDefinition { Name = "Name", ValueType = Models.Data.ValueType.Static, Case = case1 };
        var email_verified = new DataDefinition { Name = "Email_Verified", ValueType = Models.Data.ValueType.Static, InitialValue = "false", Case = case1 };
        var id = new DataDefinition { Name = "Id", ValueType = Models.Data.ValueType.UniqueIdentifier, Case = case1 };

        var nameOfPerson1 = new DataEntry
        {
            Case = case1,
            DataDefinition = name,
            Tags = new Collection<Tag> { tagCustomers, tagCustomer1 },
            Value = "Test Person"
        };

        var emailVerifiedOfPerson2 = new DataEntry
        {
            Case = case1,
            DataDefinition = email_verified,
            Tags = new Collection<Tag> { tagCustomers, tagCustomer2 }
        };

        //Case2
        var case2 = new Case { Name = "Case2", Description = "Test Case" };
        var tagCustomersCase2 = new Tag { Name = "Customers", Description = "Collection of customers", AllowedSubActions = ["Customer1_namechange"], Case = case2 };
        var tagCustomer1Case2 = new Tag { Name = "Customers_Customer1", Description = "Customer1", Case = case2, UniqueDefinition = true };

        var nameLinked = new DataDefinition { Name = "Name", ValueType = Models.Data.ValueType.Connected, ConnectionType = ConnectionType.Fulllink, PathForConnected = "Case1.{name}.Customers_Customer1", Case = case2 };
        var customerIdCalculatedAtGet = new DataDefinition { Name = "Id_Get", ValueType = Models.Data.ValueType.Calculated, ActionForCalculated = "CustomerId", Case = case2 };
        var customerIdCalculatedAtPost = new DataDefinition { Name = "Id_Post", ValueType = Models.Data.ValueType.Calculated, ActionForCalculated = "CustomerId", Case = case2 };

        var nameLinkedOfPerson1 = new DataEntry
        {
            Case = case2,
            DataDefinition = nameLinked,
            Tags = new Collection<Tag> { tagCustomersCase2, tagCustomer1Case2 }
        };

        var actionExecution = new ActionDefinition()
        {
            Case = case2,
            Name = "Customer1_namechange",
            TagUsedInAction = new List<string>() { { "Customers_Customer1" } },
            ActionFunction = "Customers_Customer1_Name[0].Value = 'Customer 1 newname'"
        };

        var actionExecutionForCalculated = new ActionDefinition()
        {
            Case = case2,
            Name = "CustomerId",
            ActionFunction = "CalculatedDataEntry.Value = (CalculatedDataEntry.Value ?? '') + '123';"
        };

        db.Cases.AddRange(case1, case2);
        db.Tags.AddRange(tagCustomers, tagCustomer1, tagCustomer2, tagCustomersCase2, tagCustomer1Case2);
        db.DataDefinitions.AddRange(name, email_verified, nameLinked, customerIdCalculatedAtGet, customerIdCalculatedAtPost, id);
        db.DataEntries.AddRange(nameOfPerson1, emailVerifiedOfPerson2, nameLinkedOfPerson1);
        db.ActionDefinitions.AddRange(actionExecution, actionExecutionForCalculated);

        await db.SaveChangesAsync();
    }
}