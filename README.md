# Delivery Fee API

A .NET API for calculating delivery fees based on weather conditions.

## Project Structure

- `App`: Contains API controllers, DTOs, and application services
- `Data`: Storage location for database files
- `Domain`: Contains business logic, interfaces, and domain models
- `Infrastructure`: Contains data access, repositories, and external services
- `Tests`: Contains unit and integration tests

## Running the Project

```bash
# Add a new migration
dotnet ef migrations add Initial --project Infrastructure --startup-project App --context AppDbContext --output-dir Persistence/Migrations

# Update the database
dotnet ef database update --project Infrastructure --startup-project App --context AppDbContext

dotnet run --project App
``` 

## Accessing Hangfire Dashboard

```bash
# Find information about the background jobs
http://localhost:5218/hangfire/recurring
```

## Work in progress
- `Caching`: Implement HybridCache for performance
- `Authentication`: Implement authentication to REST interface
- `Tests`: Further improve unit and integration tests