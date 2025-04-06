using AutoMapper;
using FlightReservationSystem.DTOs;
using FlightReservationSystem.Models;

namespace FlightReservationSystem.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Flight ↔ FlightDTO
            CreateMap<Flight, FlightDTO>()
                .ForMember(dest => dest.AirlineName, opt => opt.MapFrom(src => src.Airline.Name))
                .ForMember(dest => dest.DepartureAirport, opt => opt.MapFrom(src => src.DepartureAirport.Name))
                .ForMember(dest => dest.ArrivalAirport, opt => opt.MapFrom(src => src.ArrivalAirport.Name))
                .ForMember(dest => dest.AvailableSeats, opt => opt.MapFrom(src => src.Seats.Count(s => s.IsBooked)));

            CreateMap<FlightDTO, Flight>()
                .ForMember(dest => dest.Airline, opt => opt.Ignore())
                .ForMember(dest => dest.DepartureAirport, opt => opt.Ignore())
                .ForMember(dest => dest.ArrivalAirport, opt => opt.Ignore())
                .ForMember(dest => dest.Bookings, opt => opt.Ignore())
                .ForMember(dest => dest.Seats, opt => opt.Ignore())
                .ForMember(dest => dest.FlightId, opt => opt.Ignore());
        }
    }
}
