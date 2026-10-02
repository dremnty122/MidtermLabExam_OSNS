using CampusEvents.Models;

namespace CampusEvents.Backend;

public interface IEventRepository
{
    IReadOnlyList<EventItem> GetEvents();
    int GetSeatLimit(int eventId);
    int GetConfirmedCount(int eventId);
    bool IsRegistered(int eventId, string email);
    bool TryAddRegistration(RegistrationRecord record);
    IReadOnlyList<RegistrationRecord> GetRegistrations(int eventId);
}

public class RegistrationValidator
{
    public const string AllowedDomain = "@univ.edu.ph";
    private readonly IEventRepository _events;

    public RegistrationValidator(IEventRepository events) => _events = events;

    public bool IsValidStudentEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        email = email.Trim();
        return email.Length > AllowedDomain.Length
            && email.EndsWith(AllowedDomain, StringComparison.OrdinalIgnoreCase)
            && email.Count(c => c == '@') == 1
            && !email.Contains(' ');
    }

    public bool HasSeatAvailable(int eventId)
        => _events.GetConfirmedCount(eventId) < _events.GetSeatLimit(eventId);
}
