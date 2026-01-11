## Database (SQL Server)

This API uses **SQL Server** and **Entity Framework Core migrations**.

### Connection string

Set in `appsettings.Development.json` (or User Secrets):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=VistaTiBooks;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Creating the database

**Option A (recommended):** create the database using EF Core migrations.

Run the following command from the API project directory:

```bash
dotnet ef database update
```

This command creates the database and tables based on the existing migrations.

**Option B:** apply the DDL script manually.

Execute `database/01_create_schema.sql` using SQL Server Management Studio (SSMS) or Azure Data Studio.

### Notes

- The DDL script matches the EF Core model and constraints.
- Favorites enforces uniqueness on `(UserId, ExternalId)` to prevent duplicates.
