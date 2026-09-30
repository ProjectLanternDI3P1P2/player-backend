# MCD — Player, heroes, matchmaking, and GameSession

> Documentation for the supplied Player conceptual data model. The finalized Mermaid diagram is available in [`mcd_player.mmd`](mcd_player.mmd). The corrections required to respect the current microservice boundaries are made explicit below.

## Overview

Player owns the player's **business** identity, heroes, matchmaking queue, formed groups, and `GameSession`. It does not own the `DungeonRun`, authentication tokens, inventory, or equipment.

The model has four areas:

1. business account, hero classes, heroes, and skills;
2. command deduplication through an idempotency key;
3. matchmaking queue and group formation;
4. the game (`GameSession`), its members, and state history.

Dungeon references and technical identity data are external identifiers or claims; they are not foreign keys to Player database tables.

## 1. Player, heroes, classes, and skills

| Entity | Main attributes | Purpose |
| --- | --- | --- |
| `PLAYER` | `player_id` (PK), `display_name`, `account_status`, `new_game_plus_level`, creation and anonymization timestamps | The player's business profile and identity. |
| `HERO` | `hero_id` (PK), `player_id` (FK), `class_code` (FK), name, level, attributes, soft deletion | A hero owned by a player. |
| `HERO_CLASS` | `class_code` (PK), label, base health and mana | Hero archetype. |
| `SKILL` | `skill_code` (PK), `class_code` (FK), label, required level, targeting type | A skill offered by a class. |
| `HERO_SKILL` | `hero_id` + `skill_code` (PK/FK), `unlocked_at`, `is_active` | The association between a hero and a mastered skill. |

A `PLAYER` may own zero or more `HERO` records; each hero belongs to exactly one player. The supplied MCD imposes a business limit of ten heroes per account. Each hero has one class, and a class offers one or more skills. `HERO_SKILL` makes the many-to-many `masters` relation explicit and carries `unlocked_at` and `is_active`.

Hero deletion is soft (`is_deleted`, `deleted_at`) and must only be allowed when no active game references the hero.

## 2. Command idempotency

| Entity | Main attributes | Purpose |
| --- | --- | --- |
| `IDEMPOTENCY_KEY` | `key` (PK), `player_id` (FK), `scope`, `request_fingerprint`, `produced_resource_id`, `expires_at` | Stores the result of a replayed command. |

A player issues zero or more keys. Each important command — session creation, joining, starting, or abandonment — uses a stable key. A retry with the same scope and request fingerprint returns the already produced resource or state; reusing a key with a different payload is rejected.

The persistence model should enforce key uniqueness within its scope, for example `UNIQUE(player_id, scope, key)`, and retain the record at least until `expires_at`.

## 3. Matchmaking and group formation

| Entity | Main attributes | Purpose |
| --- | --- | --- |
| `MATCHMAKING_QUEUE_ENTRY` | `queue_entry_id` (PK), `player_id` (FK), `hero_id` (FK), status, NG+ level at entry, timestamps | A player's matchmaking request with its selected hero. |
| `MATCHMAKING_GROUP` | `group_id` (PK), status, minimum/maximum NG+ level, formation timestamp | A group formed from queue entries. |

A player may have several queue entries over its history, but each entry belongs to one player and mobilizes one hero. A group gathers compatible entries. The supplied MCD specifies **two to four active entries** per group. Mermaid cannot express that bound directly; it must be enforced transactionally when the group is formed.

The optional `results_in` relationship from `MATCHMAKING_GROUP` to `GAME_SESSION` means that an entry may never form a group and a group may never start a game. Once the session is created, its roster is locked.

## 4. GameSession and state history

| Entity | Main attributes | Purpose |
| --- | --- | --- |
| `GAME_SESSION` | `session_id` (PK), status, mode, `dungeon_run_id`, seed, timestamps, termination reason | A game owned by Player. |
| `GAME_SESSION_MEMBER` | `session_id` + `hero_id` (PK/FK), member status, join/leave timestamps | A hero's participation in a session. |
| `GAME_SESSION_TRANSITION` | `transition_id` (PK), `session_id` (FK), source/target status, cause, actor, timestamp | Audit record for session transitions. |

A `GAME_SESSION` contains one or more heroes through `GAME_SESSION_MEMBER`. This association preserves the member state and avoids attaching a hero to only one session: a hero can take part in several sessions over time.

Each session records one or more `GAME_SESSION_TRANSITION` records. A transition stores the actor, cause, and before/after states, providing the audit trail for creation, lobby, start, end, and closure.

`dungeon_run_id` identifies the `DungeonRun` owned by Dungeon. It is therefore an external value, **not a local foreign key**. Player sets it after the successful, idempotent gRPC `CreateDungeonRun` call.

When Dungeon publishes `dungeon.dungeon-run-ended.v1` through RabbitMQ, Player terminates the `GAME_SESSION`, records the corresponding transition, and emits `SessionStateChanged`. This closure is eventually consistent and can occur shortly after the dungeon ends.

## Corrections from the drawn schema

The source drawing includes email, a password hash, and a `CREDENTIAL` entity with a token hash. These elements are deliberately absent from the finalized MCD: technical authentication, tokens, and token validation belong to **Keycloak** and **Traefik**, not Player. Player receives trusted claims (`UserId`, `Username`, roles, and `CorrelationId`) and then authorizes access to its own business resources.

Likewise, `dungeon_run_id` is not a foreign key to a local `DungeonRun` table. Dungeon owns that resource. This prevents reading or writing another microservice's database.

## Business rules to enforce in use cases

- An account owns at most ten non-deleted heroes.
- A hero cannot be deleted while an active session or run references it.
- A queue entry must use a non-deleted hero owned by the requesting player.
- A forming group contains two to four active entries, and an entry belongs to at most one group at a time.
- Only the authorized creator can start or abandon the session; the roster is immutable after it is locked.
- Every state-changing command is idempotent and produces at most one effective transition.
- Dungeon-end messages must be deduplicated: RabbitMQ redeliveries cannot close a session or record a transition twice.

## MCD scope

This is a conceptual model and persistence guide. It does not lock down SQL migrations, the complete status catalogue, or gRPC and RabbitMQ contract formats. Cross-row controls — group size, hero ownership, roster locking, and concurrent deduplication — require transactions, targeted unique constraints, and appropriate application locks.
