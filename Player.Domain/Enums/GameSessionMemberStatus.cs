namespace Player.Domain.Enums;

/// <summary>Participation state of a hero in a game session, persisted by name.</summary>
public enum GameSessionMemberStatus
{
    Joined,
    Active,
    Left,
}
