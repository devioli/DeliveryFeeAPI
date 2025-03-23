# Delivery Fee API

A .NET API for calculating delivery fees based on weather conditions.

## Project Structure

- `Domain`: Contains business logic, interfaces, and domain models
- `Infrastructure`: Contains data access, repositories, and external services
- `App`: Contains API controllers, DTOs, and application services
- `Tests`: Contains unit and integration tests
- `Data`: Storage location for database files

## Entity Framework Commands

```bash
# Add a new migration
dotnet ef migrations add Initial --project Infrastructure --startup-project App --context AppDbContext --output-dir Persistence/Migrations

# Update the database
dotnet ef database update --project Infrastructure --startup-project App --context AppDbContext
```

## Running the Project

```bash
dotnet run --project App
``` 