# Player.Contracts

Versioned Protocol Buffers contract and generated C# gRPC client/server types for
the Player service.

## Installation

Configure the organisation's GitHub Packages NuGet source, then pin a released
version of the package:

```xml
<PackageReference Include="Player.Contracts" Version="0.1.1" />
```

For a C# gRPC consumer, also reference `Grpc.Net.Client`, create a channel for the
Player service's internal endpoint, then construct
`Player.Contracts.V1.PlayerService.PlayerServiceClient`.

The original `.proto` source is included in this package under `proto/`.
