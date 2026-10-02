using CampusEvents.Models;

namespace CampusEvents.Backend;

// Prototype data store. Registered as a singleton, so data resets when the app restarts.
public class InMemoryEventRepository : IEventRepository
{
    private readonly object _lock = new();
    private readonly List<RegistrationRecord> _registrations = new();

    private readonly List<EventItem> _events = new()
    {
        new EventItem { Id = 1, Title = "Intro to Generative AI Workshop", StartsAt = new DateTime(2026, 10, 14, 13, 0, 0), Venue = "Computer Lab 2", SeatLimit = 30, ImageColor = "#1f5c46", ImageAlt = "Illustration of a workshop room with laptops" },
        new EventItem { Id = 2, Title = "Career Fair 2026", StartsAt = new DateTime(2026, 10, 21, 9, 0, 0), Venue = "University Gymnasium", SeatLimit = 200, ImageColor = "#3b4f7a", ImageAlt = "Illustration of recruiter booths in a gymnasium" },
        new EventItem { Id = 3, Title = "Cybersecurity Night", StartsAt = new DateTime(2026, 10, 28, 18, 0, 0), Venue = "Lecture Hall A", SeatLimit = 2, ImageColor = "#6b3d52", ImageAlt = "Illustration of a padlock on a dark screen" },
    };

    public IReadOnlyList<EventItem> GetEvents() => _events;

    public int GetSeatLimit(int eventId)
        => _events.FirstOrDefault(e => e.Id == eventId)?.SeatLimit ?? 0;

    public int GetConfirmedCount(int eventId)
    {
        lock (_lock) return _registrations.Count(r => r.EventId == eventId);
    }

    public bool IsRegistered(int eventId, string email)
    {
        lock (_lock)
            return _registrations.Any(r => r.EventId == eventId
                && r.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    // The seat check and the add happen inside one lock, so two simultaneous
    // requests cannot both take the last seat.
    public bool TryAddRegistration(RegistrationRecord record)
    {
        lock (_lock)
        {
            if (_registrations.Count(r => r.EventId == record.EventId) >= GetSeatLimit(record.EventId))
                return false;
            _registrations.Add(record);
            return true;
        }
    }

    public IReadOnlyList<RegistrationRecord> GetRegistrations(int eventId)
    {
        lock (_lock) return _registrations.Where(r => r.EventId == eventId).ToList();
    }
}
