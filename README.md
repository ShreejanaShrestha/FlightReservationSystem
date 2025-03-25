# Flight Reservation System

An ASP.NET Core MVC web application for booking flights with user authentication.

## Features
- User roles: Admin, Traveller, Guest
- Flight search and booking
- Payment integration


## Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (or LocalDB)
- Git

## Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/ShreejanaShrestha/FlightReservationSystem.git
   ```
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Configure the database:
   - Update connection string in `appsettings.json`
   - Run migrations:
     ```bash
     dotnet ef database update
     ```
4. Restore libraries and tools
    ```bash
    dotnet tool restore
    dotnet libman restore
    ```
5. Run the application:
   ```bash
   dotnet run
   ```

## Dependencies
- ASP.NET Core 8.0
- Entity Framework Core 8.0.4
- Bootstrap 5.1