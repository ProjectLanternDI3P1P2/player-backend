namespace Player.Domain.Enums;

public static class GameSessionStatusExtensions
{
    /// <summary>A terminal session never comes back to life and no longer holds its heroes.</summary>
    public static bool IsTerminal(this GameSessionStatus status) =>
        status
            is GameSessionStatus.Failed
                or GameSessionStatus.Completed
                or GameSessionStatus.Abandoned;
}
