# Project Migration Guidelines

We use Coordinated Universal Time (UTC) — via `DateTime.UtcNow` — for all EF Core migration filenames to keep them ordered and conflict-free.

```bash
# Generate a migration (UTC timestamp):
dotnet ef migrations add AddNewTable
```

After adding a migration, check that the `yyyyMMddHHmmss` prefix matches current UTC.

**Contributors:**

* **Do not** override UTC generation with local time.
* **Always** verify the timestamp prefix on new migrations before merging.
