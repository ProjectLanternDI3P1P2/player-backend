using Grpc.Core;

namespace Player.Test.Presentation.Grpc;

/// <summary>Minimal server call context for calling a gRPC service implementation directly.</summary>
internal sealed class TestServerCallContext : ServerCallContext
{
    private TestServerCallContext(CancellationToken cancellationToken)
    {
        CancellationTokenCore = cancellationToken;
    }

    public static ServerCallContext Create() => new TestServerCallContext(CancellationToken.None);

    protected override string MethodCore => "test-method";

    protected override string HostCore => "localhost";

    protected override string PeerCore => "test-peer";

    protected override DateTime DeadlineCore => DateTime.MaxValue;

    protected override Metadata RequestHeadersCore => [];

    protected override CancellationToken CancellationTokenCore { get; }

    protected override Metadata ResponseTrailersCore => [];

    protected override Status StatusCore { get; set; }

    protected override WriteOptions? WriteOptionsCore { get; set; }

    protected override AuthContext AuthContextCore =>
        new(null, new Dictionary<string, List<AuthProperty>>());

    protected override ContextPropagationToken CreatePropagationTokenCore(
        ContextPropagationOptions? options
    ) => throw new NotSupportedException();

    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) =>
        Task.CompletedTask;
}
