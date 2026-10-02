namespace CampusEvents.Models;

public class EventItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public DateTime StartsAt { get; set; }
    public string Venue { get; set; } = "";
    public int SeatLimit { get; set; }
    public string ImageColor { get; set; } = "#1f5c46";
    public string ImageAlt { get; set; } = "";
}

public class RegistrationRecord
{
    public int EventId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
}

public record RegistrationRequest(string? FullName, string? Email);
