using FlightReservationSystem.Data;
using Microsoft.EntityFrameworkCore;
namespace FlightReservationSystem.Services
{
    /// <summary>
    /// Interface for name formatting services.
    /// Implementations provide consistent name formatting across the application.
    /// </summary>
    public interface INameFormatter
    {
        /// <summary>
        /// Properly capitalizes a passenger or user name according to business rules.
        /// </summary>
        /// <param name="name">The name to format</param>
        /// <returns>A consistently formatted name with appropriate capitalization</returns>
        string CapitalizeName(string name);
    }

    /// <summary>
    /// Utility class for generating unique Passenger Name Records (PNR).
    /// PNRs serve as user-friendly booking reference codes for customers.
    /// </summary>
    public static class PNRGenerator
    {
        /// <summary>
        /// Random number generator used for creating PNR codes.
        /// Initialized once for the lifetime of the application.
        /// </summary>
        private static readonly Random _random = new Random();

        /// <summary>
        /// Character set used for PNR generation.
        /// Includes only alphanumeric characters to avoid confusion.
        /// </summary>
        private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        /// <summary>
        /// Standard length for all generated PNR codes.
        /// Industry standard is typically 6 characters.
        /// </summary>
        private const int _pnrLength = 6;

        /// <summary>
        /// Generates a unique, random PNR code that doesn't exist in the database.
        /// Performs database checks to ensure uniqueness before returning.
        /// </summary>
        /// <param name="context">Database context for checking PNR uniqueness</param>
        /// <returns>A unique 6-character alphanumeric PNR code</returns>
        public static async Task<string> GenerateUniquePNR(ApplicationDbContext context)
        {
            string pnr;
            bool isUnique;
            do
            {
                pnr = new string(Enumerable.Repeat(_chars, _pnrLength)
                    .Select(s => s[_random.Next(s.Length)]).ToArray());
                isUnique = !await context.Bookings.AnyAsync(b => b.PNR == pnr);
            } while (!isUnique);
            return pnr;
        }
    }
}