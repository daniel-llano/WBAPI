namespace WBAPI.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private User() { }

    public static User Create(string username, string passwordHash, string role = "Invitado")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = passwordHash,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void ChangeRole(string newRole) => Role = newRole;
}
