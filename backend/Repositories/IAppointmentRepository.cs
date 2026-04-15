using TattooShop.Api.Models;

namespace TattooShop.Api.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetAppointmentAsync(string id, string artistId);
    Task AddAppointmentAsync(Appointment appointment);
    Task<IEnumerable<Appointment>> GetAppointmentsByArtistAsync(string artistId);
    Task<IEnumerable<Appointment>> GetAppointmentsByCustomerAsync(string customerUserId);
}