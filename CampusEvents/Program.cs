using CampusEvents.Backend;
using CampusEvents.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IEventRepository, InMemoryEventRepository>();
builder.Services.AddSingleton<RegistrationValidator>();

var app = builder.Build();

// Serves wwwroot/index.html at http://localhost:PORT/
app.UseDefaultFiles();
app.UseStaticFiles();

// GET /api/events : the event catalog with seats left
app.MapGet("/api/events", (IEventRepository repo) =>
    repo.GetEvents().Select(e => new
    {
        e.Id,
        e.Title,
        e.StartsAt,
        e.Venue,
        e.SeatLimit,
        SeatsLeft = e.SeatLimit - repo.GetConfirmedCount(e.Id),
        e.ImageColor,
        e.ImageAlt
    }));

// POST /api/events/{id}/registrations : register a student
app.MapPost("/api/events/{id:int}/registrations",
    (int id, RegistrationRequest req, IEventRepository repo, RegistrationValidator validator) =>
{
    var errors = new Dictionary<string, string>();
    var name = (req.FullName ?? "").Trim();
    var email = (req.Email ?? "").Trim();

    if (name.Length == 0 || name.Length > 150)
        errors["fullName"] = "Enter your full name.";
    if (!validator.IsValidStudentEmail(email))
        errors["email"] = "Use your university email ending in @univ.edu.ph.";

    var ev = repo.GetEvents().FirstOrDefault(e => e.Id == id);
    if (ev is null)
        errors["eventId"] = "Choose an event from the list.";

    if (errors.Count > 0)
        return Results.BadRequest(new { errors });

    if (repo.IsRegistered(id, email))
        return Results.Conflict(new { errors = new Dictionary<string, string> { ["email"] = "You are already registered for this event." } });

    // Seat check and add happen atomically inside the repository.
    if (!repo.TryAddRegistration(new RegistrationRecord { EventId = id, FullName = name, Email = email }))
        return Results.Conflict(new { errors = new Dictionary<string, string> { ["eventId"] = "Sorry, that event is fully booked." } });

    return Results.Created($"/api/events/{id}/registrations", new { message = $"You're registered for {ev!.Title}." });
});

// GET /api/registrations : attendee list for admins (prototype: no login)
app.MapGet("/api/registrations", (IEventRepository repo) =>
    repo.GetEvents().Select(e => new
    {
        e.Id,
        e.Title,
        e.SeatLimit,
        Attendees = repo.GetRegistrations(e.Id).Select(r => new { r.FullName, r.Email })
    }));

app.Run();
