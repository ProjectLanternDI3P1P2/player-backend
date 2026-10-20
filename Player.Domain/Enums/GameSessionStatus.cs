namespace Player.Domain.Enums;

/// <summary>Lifecycle state of a game session, persisted by name in the database.</summary>
public enum GameSessionStatus
{
    None,
    Lobby,
    Pending,
    Started,
    Active,
    Failed,
    Completed,
    Abandoned,
}
