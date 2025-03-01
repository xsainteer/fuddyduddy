# Database Migrations

## Creating a new migration

```bash
# From the Infrastructure project directory
dotnet ef migrations add MigrationName -o Data/Migrations

# Or from solution root
dotnet ef migrations add MigrationName --project src/Core/FuddyDuddy.Core.Infrastructure -o Data/Migrations
```

## Applying migrations

```bash
# From the Infrastructure project directory
dotnet ef database update

# Or from solution root
dotnet ef database update --project src/Core/FuddyDuddy.Core.Infrastructure
```

## Removing last migration

```bash
# From the Infrastructure project directory
dotnet ef migrations remove

# Or from solution root
dotnet ef migrations remove --project src/Core/FuddyDuddy.Core.Infrastructure
```

## Generate SQL script

```bash
# From the Infrastructure project directory
dotnet ef migrations script

# Or from solution root
dotnet ef migrations script --project src/Core/FuddyDuddy.Core.Infrastructure
``` 