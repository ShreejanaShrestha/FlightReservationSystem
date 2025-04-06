using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightReservationSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Flights",
                columns: new[] { "FlightId", "AirlineId", "ArrivalAirportId", "ArrivalTime", "BasePrice", "DepartureAirportId", "DepartureTime", "FlightNumber" },
                values: new object[] { 1, 1, 2, new DateTime(2025, 4, 15, 15, 0, 0, 0, DateTimeKind.Unspecified), 599.99m, 1, new DateTime(2025, 4, 15, 8, 0, 0, 0, DateTimeKind.Unspecified), "DL123" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Flights",
                keyColumn: "FlightId",
                keyValue: 1);
        }
    }
}
