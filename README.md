# Delivery Fee API

.NET Minimal API for calculating delivery fee based on location, vehicle type and weather conditions.

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

## Hangfire Dashboard

```bash
http://localhost:5218/hangfire
```

## Swagger UI

```bash
http://localhost:5218/swagger
```

## Work in progress
- `Authentication`: Implement authentication to REST interface, Hangfire Dashboard
- `Logging`