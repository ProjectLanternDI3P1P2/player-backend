# Vue d'ensemble de Player

> Document explicatif du service **Player** : responsabilités, frontières et principaux échanges avec les autres services.

Player est responsable de l'identité métier du joueur et de la préparation, puis du cycle de vie, d'une partie. Il gère notamment :

- l'identité et les droits métier du joueur ;
- le compte et le profil ;
- les héros ;
- le matchmaking et le lobby ;
- la `GameSession` ;
- les échanges avec Dungeon pour démarrer ou arrêter une partie.

Player ne gère pas l'authentification technique (Keycloak et Traefik), l'inventaire ou l'équipement (Rewards), la progression (Progression), le contenu ou l'état du donjon (Dungeon), ni la résolution des combats (Combat).

## 1. Authentification et autorisation

L'authentification technique est extérieure à Player :

```text
Client -> Traefik -> Keycloak -> Player
```

Keycloak fournit le jeton d'identité. Traefik le valide et transmet à Player des claims de confiance : `UserId`, `Username`, rôles et `CorrelationId`. Player ne gère donc ni les jetons ni la cryptographie.

Player vérifie ensuite que le joueur est autorisé à utiliser une ressource :

1. **Droit fonctionnel** : le rôle permet-il d'appeler cet endpoint ?
2. **Droit sur la ressource** : le joueur est-il bien propriétaire ou membre de la ressource concernée ?

Même avec un jeton valide, un joueur ne peut pas accéder au héros, à la session, au groupe SignalR ou au combat d'un autre joueur.

> Keycloak authentifie ; Player autorise l'accès aux ressources métier.

## 2. Compte et profil

Player est la source de vérité de l'identité métier du joueur. Il possède le `UserId`, le nom affiché, le handle public, le niveau NG+ et le statut du compte (il n'y a pas encore d'avatar ni d'autres informations de profil en base). Ces données sont notamment disponibles sous `/api/player/...`.

### Profil public (US-PLAYER-06)

Exception à la règle d'identité de la section 1 : le profil public est servi **sans identité**. Le contrôleur `PublicProfilesController` n'utilise pas `ICurrentPlayerResolver` (qui exige `X-User-Id` et crée le joueur) et ne lit aucun en-tête `X-User-Id`, `X-Username` ou `X-Roles`.

| Route | Rôle |
| --- | --- |
| `GET`/`HEAD /api/player/v1/players/{playerId:guid}/public-profile` | Profil public par id. |
| `GET`/`HEAD /api/player/v1/players/by-handle/{handle}/public-profile` | Profil public par handle (insensible à la casse). |

- **Champs exposés** : `playerId`, `publicHandle`, `displayName`, `newGamePlusLevel`, `heroes[]` (`name`, `classCode`, `className`, `level`, héros non supprimés, triés par niveau décroissant puis nom), `counters` (`heroCount`, `highestHeroLevel`, `completedSessionCount`). Jamais `ExternalUserId`, statut, dates, stats ni apparence. Un test de sérialisation verrouille cette liste.
- **`completedSessionCount`** compte les `GameSession` au statut `Completed` jouées par un héros du joueur, héros supprimés compris. Il vaut 0 tant que rien ne pose `Completed`.
- **404 uniforme** (ProblemDetails) pour un joueur inconnu, anonymisé (`anonymized_at` renseigné), au statut différent de `Active`, ou un handle mal formé (sans requête en base) : on ne peut pas énumérer les comptes.
- **Cache** : 200 → `Cache-Control: public, max-age=60, stale-while-revalidate=300` ; 404 → `Cache-Control: public, max-age=60`.
- **Gateway** : ces deux routes doivent être ouvertes sans JWT dans Traefik (dev et prod), et rien de plus large sous `/api/player/`.
- **Handle** : généré à la création du joueur et rempli pour les joueurs existants par la migration `AddPlayerPublicHandle` (même règle en SQL, voir `PlayerPublicHandle`). Le backfill suppose une base Postgres encodée en UTF8 (`normalize`).

Les autres services peuvent consulter Player lorsqu'ils doivent afficher l'identité d'un joueur. Par exemple, Rewards ou Progression peut connaître un `UserId` et demander à Player le nom du joueur associé.

Player fournit l'identité ; il ne possède ni l'inventaire, ni l'équipement, ni la progression.

## 3. Héros

Player est également la source de vérité des héros. Il gère leur création, leur nom, leur classe, leur sélection, leur lecture et leur association à une partie. Un compte peut posséder jusqu'à dix héros.

| Donnée | Service responsable |
| --- | --- |
| Identité du joueur | Player |
| Identité du héros | Player |
| Équipement | Rewards |
| Inventaire | Rewards |
| Progression | Progression |

Un héros ne peut pas être supprimé tant qu'il est référencé par une partie active : la partie doit se terminer et les références doivent être libérées. Cette règle évite de supprimer un héros encore utilisé par un `DungeonRun` ou un combat.

## 4. Snapshots au démarrage d'un combat

Lorsqu'un combat commence, Combat doit disposer d'un état figé du personnage :

```text
Player  -- GetCombatantSnapshot ----------> Combat
Rewards -- GetCombatInventorySnapshot ----> Combat
```

Combat combine les deux snapshots : héros et équipement forment l'état initial du combattant. Une fois le combat commencé, cet état reste figé ; une modification ultérieure de l'équipement ne modifie donc pas le combat en cours. Cela garantit cohérence, déterminisme et équité.

Si Player ou Rewards ne peut pas fournir son snapshot, le combat ne démarre pas. Combat ne doit jamais être initialisé avec un état partiel.

## 5. Matchmaking, lobby et roster

Player prépare la partie selon le flux suivant :

```text
Matchmaking
  -> Lobby
  -> sélection des joueurs
  -> sélection des héros
  -> verrouillage du roster
  -> GameSession
  -> Dungeon
```

Dans le lobby, les joueurs peuvent rejoindre, quitter ou changer de héros. Lorsque le créateur démarre la partie, le roster est définitivement verrouillé, puis transmis à Dungeon. Ce verrouillage évite les arrivées ou départs pendant la création du donjon et simplifie la concurrence entre Player, Dungeon et Combat.

## 6. GameSession

La `GameSession` représente une partie du joueur. Elle est distincte de la session d'authentification et du `DungeonRun`.

Player en gère le cycle de vie complet :

```text
Création -> Lobby -> Démarrage -> DungeonRun -> Fin -> Fermeture
```

Au démarrage, le créateur lance la session et Player appelle `CreateDungeonRun` sur Dungeon. Cet appel est idempotent : un retry réseau de la même commande ne doit pas créer un second donjon.

## 7. Communication entre Player et Dungeon

Deux mécanismes sont utilisés selon le sens de communication.

### Player vers Dungeon : gRPC synchrone

Player demande à Dungeon de créer ou d'abandonner un run et obtient rapidement un accusé de réception :

```text
Player -- CreateDungeonRun / AbandonDungeonRun --> Dungeon
```

### Dungeon vers Player : RabbitMQ asynchrone

Quand un donjon se termine, Dungeon publie l'événement `dungeon.dungeon-run-ended.v1` dans RabbitMQ. Player le consomme, traite le résultat (victoire, défaite ou abandon), ferme la `GameSession` et informe le client avec `SessionStateChanged`.

```text
Dungeon -> RabbitMQ -> Player -> SessionStateChanged
```

La fermeture est donc éventuellement cohérente : un court délai peut séparer la fin du donjon et la clôture officielle de la `GameSession` par Player.

## 8. REST et SignalR

Player emploie deux modes de communication avec le client.

| Mode | Usage |
| --- | --- |
| REST | Profil, héros, création ou jonction de ressources et opérations de préparation hors gameplay. |
| SignalR / `PlayerHub` | Création, jonction, démarrage et abandon de session, ainsi que diffusion des changements d'état temps réel. |

## 9. Idempotence des commandes

Toute commande importante porte un `CommandId`.

```text
Client -- StartGame(CommandId=123) --> Player
```

Si le client renvoie exactement la même commande — à cause d'un problème réseau, d'un double-clic ou d'un message rejoué — Player la reconnaît, ne rejoue pas l'opération et retourne l'état déjà obtenu.

## 10. Responsabilités par service

| Service | Responsabilité principale |
| --- | --- |
| Keycloak | Authentification |
| Traefik | Validation du jeton et transmission des claims |
| Player | Joueur, héros, matchmaking et `GameSession` |
| Rewards | Inventaire et équipement |
| Progression | Progression |
| Dungeon | Génération et déroulement du donjon |
| Combat | État et résolution des combats |
| RabbitMQ | Transport des événements asynchrones |

## En une phrase

Player possède l'identité, les héros, le lobby et la `GameSession`. Il prépare la partie, fige les participants, demande à Dungeon de créer le donjon puis clôture la session lorsque Dungeon lui signale sa fin.
