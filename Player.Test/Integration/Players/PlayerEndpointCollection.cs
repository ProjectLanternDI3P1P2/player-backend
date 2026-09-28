namespace Player.Test.Integration.Players;

[CollectionDefinition(Name)]
public sealed class PlayerEndpointCollection : ICollectionFixture<PlayerEndpointFixture>
{
    public const string Name = "Player endpoint integration tests";
}
