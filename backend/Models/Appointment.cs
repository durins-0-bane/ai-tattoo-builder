using Newtonsoft.Json;

namespace TattooShop.Api.Models;

public record Appointment(
    [property: JsonProperty("id")] string Id,
    [property: JsonProperty("partitionKey")] string ArtistId,
    string CustomerName,
    string CustomerEmail,
    DateTime StartTime,
    int DurationMinutes,
    string ServiceType, // e.g., "Consultation", "Tattoo Session"
    string Status // e.g., "Pending", "Confirmed", "Completed"
);