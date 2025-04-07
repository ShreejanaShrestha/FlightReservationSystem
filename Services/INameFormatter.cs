using FlightReservationSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace FlightReservationSystem.Services
{
    public interface INameFormatter
    {
        string CapitalizeName(string name);
    }

    public static class PNRGenerator
    {
        private static readonly Random _random = new Random();
        private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int _pnrLength = 6;

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
