using Microsoft.Data.SqlClient;

namespace CampusEvents.Backend;

public class RegistrationService
{
    private readonly string _connectionString;

    // The connection string is injected (appsettings / user-secrets / env vars),
    // never hard-coded in source.
    public RegistrationService(string connectionString)
    {
        _connectionString = connectionString
            ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Returns the most recent registration ID for the given email,
    /// or null if the user has no registration.
    /// </summary>
    public string? GetUserRegistration(string inputEmail)
    {
        if (string.IsNullOrWhiteSpace(inputEmail))
            throw new ArgumentException("Email is required.", nameof(inputEmail));

        // 'using' guarantees Dispose()/Close() even if an exception is thrown.
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        // Parameterized query: user input is sent as data, never concatenated into SQL.
        const string sql = @"
            SELECT TOP (1) r.RegistrationId
            FROM dbo.Registrations AS r
            INNER JOIN dbo.Users AS u ON u.UserId = r.UserId
            WHERE u.Email = @Email
            ORDER BY r.RegisteredAt DESC;";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 254).Value = inputEmail;

        // ExecuteScalar returns null when no row matches; avoid NullReferenceException.
        return cmd.ExecuteScalar()?.ToString();
    }
}
