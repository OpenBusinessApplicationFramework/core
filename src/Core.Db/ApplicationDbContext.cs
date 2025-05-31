using Core.Models.Action;
using Core.Models.Common;
using Core.Models.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Core.Db;

public class ApplicationDbContext : DbContext
{
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Case> Cases { get; set; } = null!;
    public DbSet<DataDefinition> DataDefinitions { get; set; } = null!;
    public DbSet<DataEntry> DataEntries { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<Models.Data.DataSet> DataSets { get; set; } = null!;
    public DbSet<ActionDefinition> ActionDefinitions { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Case>().HasIndex(e => e.Name).IsUnique();
        modelBuilder.Entity<DataDefinition>().HasIndex(e => e.Name).IsUnique();
        modelBuilder.Entity<Tag>().HasIndex(e => e.Name).IsUnique();
        modelBuilder.Entity<Models.Data.DataSet>().HasIndex(e => e.Name).IsUnique();
        modelBuilder.Entity<ActionDefinition>().HasIndex(e => e.Name).IsUnique();

        modelBuilder.Entity<DataEntry>().HasIndex(e => new { e.DataDefinitionId, e.DataSetId, e.CaseId }).IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}