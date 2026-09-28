# Player overview

> Explanatory documentation for the **Player** service: responsibilities, boundaries, and core interactions with the other services.

Player owns the player's business identity and the preparation and lifecycle of a game. It manages business authorization, the account and profile, heroes, matchmaking and lobby, the `GameSession`, and the calls to Dungeon that start or stop a game.

Player does not manage technical authentication (Keycloak and Traefik), inventory or equipment (Rewards), progression (Progression), dungeon content or state (Dungeon), or combat resolution (Combat).

## 1. Authentication and authorization

Technical authentication is outside Player:

```text
Client -> Traefik -> Keycloak -> Player
```

Keycloak issues the identity token. Traefik validates it and forwards trusted claims to Player: `UserId`, `Username`, roles, and `CorrelationId`. Player therefore neither handles tokens nor performs cryptography.

Player authorizes access to its business resources at two levels:

1. **Functional permission**: does the role allow the endpoint to be called?
2. **Resource permission**: does the player own or belong to the resource?

A valid token never permits access to another player's hero, session, SignalR group, or combat.

> Keycloak authenticates; Player authorizes business-resource access.

## 2. Account and profile

Player is the source of truth for business player identity: `UserId`, username, avatar, profile information, and general account information. These details are exposed through `/api/player/...`.

Other services may query Player when they need to display a player's identity. For example, Rewards or Progression can know a `UserId` and ask Player for its display name. Player provides identity only; it does not own inventory, equipment, or progression.

## 3. Heroes

Player is the source of truth for heroes. It manages their creation, name, class, selection, reading, and participation in a game. An account may own up to ten heroes.

| Data | Owning service |
| --- | --- |
| Player identity | Player |
| Hero identity | Player |
| Equipment | Rewards |
| Inventory | Rewards |
| Progression | Progression |

A hero cannot be deleted while an active game references it. The game must end and its references be released first, preventing deletion while a `DungeonRun` or combat still uses the hero.

## 4. Combat-start snapshots

When combat starts, Combat needs a frozen character state:

```text
Player  -- GetCombatantSnapshot ----------> Combat
Rewards -- GetCombatInventorySnapshot ----> Combat
```

Combat combines both snapshots: hero and equipment form the combatant's initial state. Once combat has started, this state is frozen, so a later equipment change cannot affect the running combat. This provides consistency, determinism, and fairness.

If Player or Rewards cannot provide its snapshot, the combat must not start. Combat must never initialize from partial data.

## 5. Matchmaking, lobby, and roster

```text
Matchmaking
  -> Lobby
  -> player selection
  -> hero selection
  -> roster lock
  -> GameSession
  -> Dungeon
```

In the lobby, players can join, leave, or change heroes. When the creator starts the game, the roster is permanently locked and sent to Dungeon. This prevents arrivals or departures while the dungeon is being created and simplifies concurrency across Player, Dungeon, and Combat.

## 6. GameSession

A `GameSession` is a player's game. It is distinct from an authentication session and from a `DungeonRun`.

```text
Creation -> Lobby -> Start -> DungeonRun -> End -> Close
```

At start, the creator starts the session and Player calls `CreateDungeonRun` on Dungeon. The call is idempotent: retrying the same network command must not create a second dungeon.

## 7. Player and Dungeon communication

Player calls Dungeon synchronously through gRPC to create or abandon a run:

```text
Player -- CreateDungeonRun / AbandonDungeonRun --> Dungeon
```

Dungeon notifies Player asynchronously through RabbitMQ when a dungeon ends:

```text
Dungeon -> RabbitMQ -> Player -> SessionStateChanged
```

Player consumes `dungeon.dungeon-run-ended.v1`, handles the outcome (victory, defeat, or abandonment), closes the `GameSession`, and notifies the client through `SessionStateChanged`. Closure is eventually consistent: a short delay may exist between the dungeon ending and Player officially closing the session.

## 8. REST and SignalR

| Mode | Use |
| --- | --- |
| REST | Profile and hero management, resource creation or joining, and other preparation operations outside gameplay. |
| SignalR / `PlayerHub` | Session creation, joining, starting, abandoning, and real-time state-change delivery. |

## 9. Command idempotency

Every important command carries a `CommandId`:

```text
Client -- StartGame(CommandId=123) --> Player
```

If the client resends the same command because of a network problem, double-click, or replayed message, Player recognizes it, does not repeat the operation, and returns the state already obtained.

## 10. Service responsibilities

| Service | Main responsibility |
| --- | --- |
| Keycloak | Authentication |
| Traefik | Token validation and trusted-claim forwarding |
| Player | Player, heroes, matchmaking, and `GameSession` |
| Rewards | Inventory and equipment |
| Progression | Progression |
| Dungeon | Dungeon generation and execution |
| Combat | Combat state and resolution |
| RabbitMQ | Asynchronous event transport |

## In one sentence

Player owns identity, heroes, the lobby, and the `GameSession`: it prepares the game, freezes the participants, asks Dungeon to create the dungeon, then closes the session when Dungeon reports its end.
