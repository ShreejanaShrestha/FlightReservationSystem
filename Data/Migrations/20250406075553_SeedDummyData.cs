using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FlightReservationSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDummyData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First delete seats linked to flights
            migrationBuilder.Sql(@"
    DELETE FROM Seats
    WHERE FlightId IN (
        SELECT FlightId FROM Flights
        WHERE DepartureAirportId = 3 OR ArrivalAirportId = 3
    );
");

            // Then delete flights referencing that airport
            migrationBuilder.Sql(@"
    DELETE FROM Flights
    WHERE DepartureAirportId = 3 OR ArrivalAirportId = 3;
");

            // Now safely delete the airport
            migrationBuilder.Sql("DELETE FROM Airports WHERE AirportId = 3;");

            migrationBuilder.InsertData(
                table: "Airports",
                columns: new[] { "AirportId", "City", "Code", "Country", "Name" },
                values: new object[,]
                {
                    { 3, "Los Angeles", "LAX", "USA", "Los Angeles International" },
                    { 4, "Paris", "CDG", "France", "Charles de Gaulle Airport" },
                    { 5, "Dubai","DXB", "UAE", "Dubai International" },
                    { 6, "Sydney", "SYD", "Australia", "Sydney Airport" },
                    { 7, "Tokyo", "HND", "Japan", "Haneda Airport" },
                    { 8, "Singapore", "SIN", "Singapore", "Changi Airport" },
                    { 9, "Frankfurt", "FRA", "Germany", "Frankfurt Airport" },
                    { 10, "Toronto", "YYZ", "Canada", "Toronto Pearson International" }
                });
            migrationBuilder.InsertData(
                table: "Airlines",
                columns: new[] { "AirlineId", "Name", "Code", "LogoUrl" },
                values: new object[,]
                {
                    { 1, "Delta Airlines", "DL", "/images/airlines/delta.png" },
                    { 2, "United Airlines", "UA", "/images/airlines/united.png" },
                    { 3, "American Airlines", "AA", "/images/airlines/american.png" },
                    { 4, "Emirates", "EK", "/images/airlines/emirates.png" },
                    { 5, "Qatar Airways", "QR", "/images/airlines/qatar.png" },
                    { 6, "Lufthansa", "LH", "/images/airlines/lufthansa.png" },
                    { 7, "Air France", "AF", "/images/airlines/airfrance.png" },
                    { 8, "British Airways", "BA", "/images/airlines/british.png" },
                    { 9, "Singapore Airlines", "SQ", "/images/airlines/singapore.png" },
                    { 10, "Japan Airlines", "JL", "/images/airlines/japan.png" }
                });

            migrationBuilder.InsertData(
                table: "Flights",
                columns: new[] { "FlightId", "FlightNumber", "AirlineId", "DepartureAirportId", "ArrivalAirportId", "DepartureTime", "ArrivalTime", "BasePrice" },
                values: new object[,]
                {
                    { 1, "DL123", 1, 1, 3, DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(5), 299.99m },
                    { 2, "UA456", 2, 3, 2, DateTime.Now.AddDays(2), DateTime.Now.AddDays(2).AddHours(11), 599.99m },
                    { 3, "AA789", 3, 4, 1, DateTime.Now.AddDays(3), DateTime.Now.AddDays(3).AddHours(7), 399.99m },
                    { 4, "EK101", 4, 5, 6, DateTime.Now.AddDays(4), DateTime.Now.AddDays(4).AddHours(14), 899.99m },
                    { 5, "QR202", 5, 7, 5, DateTime.Now.AddDays(5), DateTime.Now.AddDays(5).AddHours(9), 799.99m },
                    { 6, "LH303", 6, 8, 9, DateTime.Now.AddDays(6), DateTime.Now.AddDays(6).AddHours(2), 349.99m },
                    { 7, "AF404", 7, 10, 4, DateTime.Now.AddDays(7), DateTime.Now.AddDays(7).AddHours(8), 449.99m },
                    { 8, "BA505", 8, 2, 7, DateTime.Now.AddDays(8), DateTime.Now.AddDays(8).AddHours(12), 699.99m },
                    { 9, "SQ606", 9, 6, 10, DateTime.Now.AddDays(9), DateTime.Now.AddDays(9).AddHours(16), 999.99m },
                    { 10, "JL707", 10, 9, 8, DateTime.Now.AddDays(10), DateTime.Now.AddDays(10).AddHours(3), 499.99m }
                });
            
            // Seats (10 records per flight = 100 total)
            for (int flightId = 1; flightId <= 10; flightId++)
            {
                for (int seatNum = 1; seatNum <= 10; seatNum++)
                {
                    migrationBuilder.InsertData(
                        table: "Seats",
                        columns: new[] { "FlightId", "SeatNumber", "Class", "IsBooked" },
                        values: new object[]
                        {
                            flightId,
                            seatNum <= 2 ? $"A{seatNum}" :
                            seatNum <= 6 ? $"B{seatNum-2}" : $"C{seatNum-6}",
                            seatNum <= 2 ? "First" : seatNum <= 6 ? "Business" : "Economy",
                            false
                        });
                }
            }
            migrationBuilder.InsertData(
    table: "Airports",
    columns: new[] { "AirportId", "City", "Code", "Country", "Name" },
    values: new object[,]
    {
        { 11, "London", "LHR", "UK", "Heathrow Airport" },
        { 12, "Hong Kong", "HKG", "China", "Hong Kong International" },
        { 13, "Amsterdam", "AMS", "Netherlands", "Schiphol Airport" },
        { 14, "Istanbul", "IST", "Turkey", "Istanbul Airport" },
        { 15, "Seoul", "ICN", "South Korea", "Incheon International" },
        { 16, "Mumbai", "BOM", "India", "Chhatrapati Shivaji International" },
        { 17, "Rome", "FCO", "Italy", "Leonardo da Vinci-Fiumicino" },
        { 18, "New York", "JFK", "USA", "John F. Kennedy International" },
        { 19, "Chicago", "ORD", "USA", "O'Hare International" },
        { 20, "San Francisco", "SFO", "USA", "San Francisco International" }
    });
            migrationBuilder.InsertData(
    table: "Airlines",
    columns: new[] { "AirlineId", "Name", "Code", "LogoUrl" },
    values: new object[,]
    {
        { 11, "Cathay Pacific", "CX", "/images/airlines/cathay.png" },
        { 12, "Qantas", "QF", "/images/airlines/qantas.png" },
        { 13, "Turkish Airlines", "TK", "/images/airlines/turkish.png" },
        { 14, "Air Canada", "AC", "/images/airlines/aircanada.png" },
        { 15, "ANA", "NH", "/images/airlines/ana.png" },
        { 16, "KLM", "KL", "/images/airlines/klm.png" },
        { 17, "Swiss International", "LX", "/images/airlines/swiss.png" },
        { 18, "Etihad Airways", "EY", "/images/airlines/etihad.png" },
        { 19, "Virgin Atlantic", "VS", "/images/airlines/virgin.png" },
        { 20, "Air India", "AI", "/images/airlines/airindia.png" }
    });
            migrationBuilder.InsertData(
    table: "Flights",
    columns: new[] { "FlightId", "FlightNumber", "AirlineId", "DepartureAirportId", "ArrivalAirportId", "DepartureTime", "ArrivalTime", "BasePrice" },
    values: new object[,]
    {
        // Transatlantic flights
        { 11, "BA117", 8, 11, 18, DateTime.Now.AddDays(1).AddHours(9), DateTime.Now.AddDays(1).AddHours(14), 599.99m },
        { 12, "VS45", 19, 18, 11, DateTime.Now.AddDays(2).AddHours(18), DateTime.Now.AddDays(3).AddHours(7), 649.99m },
        { 13, "DL84", 1, 18, 13, DateTime.Now.AddDays(3).AddHours(11), DateTime.Now.AddDays(3).AddHours(23), 529.99m },
        
        // Asia-Pacific flights
        { 14, "SQ321", 9, 11, 12, DateTime.Now.AddDays(4).AddHours(21), DateTime.Now.AddDays(5).AddHours(17), 899.99m },
        { 15, "CX831", 11, 12, 18, DateTime.Now.AddDays(5).AddHours(8), DateTime.Now.AddDays(5).AddHours(20), 849.99m },
        { 16, "QF2", 12, 6, 11, DateTime.Now.AddDays(6).AddHours(10), DateTime.Now.AddDays(6).AddHours(23), 1099.99m },
        
        // Middle East connections
        { 17, "EK203", 4, 5, 11, DateTime.Now.AddDays(7).AddHours(3), DateTime.Now.AddDays(7).AddHours(8), 699.99m },
        { 18, "QR15", 5, 5, 13, DateTime.Now.AddDays(8).AddHours(2), DateTime.Now.AddDays(8).AddHours(7), 659.99m },
        { 19, "TK1981", 13, 14, 15, DateTime.Now.AddDays(9).AddHours(12), DateTime.Now.AddDays(9).AddHours(22), 599.99m },
        
        // Domestic US flights
        { 20, "AA245", 3, 18, 3, DateTime.Now.AddDays(10).AddHours(7), DateTime.Now.AddDays(10).AddHours(10).AddMinutes(30), 199.99m },
        { 21, "UA1203", 2, 3, 19, DateTime.Now.AddDays(11).AddHours(9), DateTime.Now.AddDays(11).AddHours(12).AddMinutes(45), 179.99m },
        { 22, "DL677", 1, 20, 18, DateTime.Now.AddDays(12).AddHours(14), DateTime.Now.AddDays(12).AddHours(22), 249.99m },
        
        // European connections
        { 23, "LH457", 6, 13, 9, DateTime.Now.AddDays(13).AddHours(8), DateTime.Now.AddDays(13).AddHours(9).AddMinutes(30), 129.99m },
        { 24, "AF1789", 7, 4, 17, DateTime.Now.AddDays(14).AddHours(16), DateTime.Now.AddDays(14).AddHours(18), 159.99m },
        { 25, "KL1666", 16, 13, 8, DateTime.Now.AddDays(15).AddHours(10), DateTime.Now.AddDays(15).AddHours(11), 139.99m },
        
        // Asian connections
        { 26, "JL45", 10, 7, 15, DateTime.Now.AddDays(16).AddHours(13), DateTime.Now.AddDays(16).AddHours(15).AddMinutes(30), 229.99m },
        { 27, "NH811", 15, 15, 12, DateTime.Now.AddDays(17).AddHours(9), DateTime.Now.AddDays(17).AddHours(12), 279.99m },
        { 28, "AI131", 20, 16, 5, DateTime.Now.AddDays(18).AddHours(23), DateTime.Now.AddDays(19).AddHours(4), 349.99m },
        
        // More long-haul flights
        { 29, "AC888", 14, 10, 11, DateTime.Now.AddDays(19).AddHours(20), DateTime.Now.AddDays(20).AddHours(9), 599.99m },
        { 30, "EY11", 18, 5, 20, DateTime.Now.AddDays(20).AddHours(2), DateTime.Now.AddDays(20).AddHours(15), 799.99m },
        { 31, "BA117", 8, 11, 18, DateTime.Now.AddDays(3).AddHours(9), DateTime.Now.AddDays(3).AddHours(14), 499.79m },
        { 32, "VS45", 19, 18, 11, DateTime.Now.AddDays(4).AddHours(18), DateTime.Now.AddDays(3).AddHours(7), 349.69m },
        { 33, "DL84", 1, 18, 13, DateTime.Now.AddDays(5).AddHours(11), DateTime.Now.AddDays(4).AddHours(23), 429.99m },


    });

            // Generate seats for each flight (11-30)
            for (int flightId = 11; flightId <= 30; flightId++)
            {
                // First Class (2 seats)
                for (int i = 1; i <= 2; i++)
                {
                    migrationBuilder.InsertData(
                        table: "Seats",
                        columns: new[] { "FlightId", "SeatNumber", "Class", "IsBooked" },
                        values: new object[] { flightId, $"F{i}", "First", false });
                }

                // Business Class (8 seats)
                for (int i = 1; i <= 8; i++)
                {
                    migrationBuilder.InsertData(
                        table: "Seats",
                        columns: new[] { "FlightId", "SeatNumber", "Class", "IsBooked" },
                        values: new object[] { flightId, $"B{i}", "Business", false });
                }

                // Economy Class (30 seats)
                for (int i = 1; i <= 30; i++)
                {
                    migrationBuilder.InsertData(
                        table: "Seats",
                        columns: new[] { "FlightId", "SeatNumber", "Class", "IsBooked" },
                        values: new object[] { flightId, $"E{i}", "Economy", new Random().Next(0, 5) == 0 }); // 20% chance of being booked
                }
            }


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Airports",
                keyColumn: "AirportId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Airports",
                keyColumn: "AirportId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Flights",
                keyColumn: "FlightId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Flights",
                keyColumn: "FlightId",
                keyValue: 2);
            migrationBuilder.Sql("DELETE FROM Seats");
            migrationBuilder.Sql("DELETE FROM Flights");
            migrationBuilder.Sql("DELETE FROM Airlines");
            migrationBuilder.Sql("DELETE FROM Airports");

        }
    }
}
