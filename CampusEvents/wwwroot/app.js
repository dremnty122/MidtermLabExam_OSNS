// Frontend talks to the C# API (Program.cs) with fetch().
const $ = (id) => document.getElementById(id);
const esc = (s) => String(s).replace(/[&<>"']/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));

// Inline SVG placeholder picture so images work offline and still have alt text.
const art = (c) => "data:image/svg+xml," + encodeURIComponent(
  `<svg xmlns='http://www.w3.org/2000/svg' width='400' height='160'><rect width='400' height='160' fill='${c}'/><circle cx='320' cy='60' r='40' fill='rgba(255,255,255,.25)'/></svg>`);

async function loadEvents() {
  const res = await fetch("/api/events");
  if (!res.ok) throw new Error("Could not load events.");
  const events = await res.json();

  $("catalog").innerHTML = events.map((e) => `
    <article>
      <img src="${art(e.imageColor)}" alt="${esc(e.imageAlt)}">
      <h3>${esc(e.title)}</h3>
      <p>${esc(new Date(e.startsAt).toLocaleString([], { dateStyle: "medium", timeStyle: "short" }))}</p>
      <p>${esc(e.venue)}</p>
      <p class="seats ${e.seatsLeft <= 0 ? "full" : ""}">${e.seatsLeft <= 0 ? "Fully booked" : e.seatsLeft + " seats left"}</p>
    </article>`).join("");

  const selected = $("event-select").value;
  $("event-select").innerHTML = '<option value="">Select an event</option>' +
    events.map((e) => `<option value="${e.id}" ${e.seatsLeft <= 0 ? "disabled" : ""}>${esc(e.title)}</option>`).join("");
  $("event-select").value = selected;
}

async function loadAttendees() {
  const res = await fetch("/api/registrations");
  if (!res.ok) throw new Error("Could not load attendees.");
  const events = await res.json();
  $("attendees").innerHTML = events.map((e) => `
    <table>
      <caption>${esc(e.title)} (${e.attendees.length}/${e.seatLimit})</caption>
      <thead><tr><th scope="col">Name</th><th scope="col">Email</th></tr></thead>
      <tbody>${e.attendees.length
        ? e.attendees.map((a) => `<tr><td>${esc(a.fullName)}</td><td>${esc(a.email)}</td></tr>`).join("")
        : '<tr><td colspan="2">No registrations yet.</td></tr>'}</tbody>
    </table>`).join("");
}

function showErrors(errors = {}) {
  $("name-err").textContent = errors.fullName || "";
  $("email-err").textContent = errors.email || "";
  $("event-err").textContent = errors.eventId || "";
}

$("register-form").addEventListener("submit", async (ev) => {
  ev.preventDefault();
  const status = $("form-status");
  const eventId = $("event-select").value;

  if (!eventId) { showErrors({ eventId: "Choose an event." }); status.textContent = ""; return; }

  try {
    const res = await fetch(`/api/events/${encodeURIComponent(eventId)}/registrations`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ fullName: $("full-name").value, email: $("email").value })
    });
    const data = await res.json();

    if (!res.ok) {
      showErrors(data.errors);
      status.className = "status bad";
      status.textContent = "";
      return;
    }
    showErrors();
    status.className = "status ok";
    status.textContent = data.message;
    ev.target.reset();
    await loadEvents();
  } catch {
    status.className = "status bad";
    status.textContent = "Could not reach the server. Is dotnet run still running?";
  }
});

function showView(admin) {
  $("student-view").hidden = admin;
  $("admin-view").hidden = !admin;
  $("tab-student").setAttribute("aria-pressed", String(!admin));
  $("tab-admin").setAttribute("aria-pressed", String(admin));
  (admin ? loadAttendees() : loadEvents()).catch(() => {});
}
$("tab-student").addEventListener("click", () => showView(false));
$("tab-admin").addEventListener("click", () => showView(true));

loadEvents().catch(() => {
  $("catalog").innerHTML = "<p>Could not load events. Start the server with <code>dotnet run</code>.</p>";
});
