namespace Player.Domain.Entities;

public sealed class GameSessionMember
{
    public Guid SessionId { get; set; }
    public Guid HeroId { get; set; }
    public string MemberStatus { get; set; } = string.Empty;
    public DateTimeOffset JoinedAt { get; set; }
    public DateTimeOffset? LeftAt { get; set; }
    public GameSession GameSession { get; set; } = null!;
    public Hero Hero { get; set; } = null!;
}
