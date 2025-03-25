using System.Globalization;

namespace FlightReservationSystem.Services
{
    public class NameFormattingService : INameFormatter
    {
        public string CapitalizeName(string name)
        {
            return CultureInfo.CurrentCulture.TextInfo
                .ToTitleCase(name?.ToLower().Trim() ?? "");
        }
    }
}
