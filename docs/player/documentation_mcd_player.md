# MCD — Player, héros, matchmaking et GameSession

> Documentation du MCD Player fourni. Le diagramme Mermaid finalisé est dans [`mcd_player.mmd`](mcd_player.mmd). Les corrections nécessaires pour respecter les frontières actuelles des microservices sont explicitées ci-dessous.

## Vue d'ensemble

Player est propriétaire de l'identité **métier** du joueur, des héros, de la file de matchmaking, des groupes formés et de la `GameSession`. Il ne possède ni le `DungeonRun`, ni les jetons d'authentification, ni l'inventaire ou l'équipement.

Le modèle se découpe en quatre zones :

1. compte métier, classes, héros et compétences ;
2. déduplication des commandes par clé d'idempotence ;
3. file de matchmaking et formation d'un groupe ;
4. partie (`GameSession`), ses participants et son historique d'états.

Les références vers Dungeon ou l'identité technique sont des identifiants ou claims externes ; elles ne sont pas des clés étrangères vers des tables de la base Player.

## 1. Joueur, héros, classes et compétences

| Entité | Attributs principaux | Rôle |
| --- | --- | --- |
| `PLAYER` | `player_id` (PK), `display_name`, `account_status`, `new_game_plus_level`, dates de création et d'anonymisation | Profil et identité métier du joueur. |
| `HERO` | `hero_id` (PK), `player_id` (FK), `class_code` (FK), nom, niveau, caractéristiques, suppression logique | Héros appartenant à un joueur. |
| `HERO_CLASS` | `class_code` (PK), libellé, PV et mana de base | Archétype d'un héros. |
| `SKILL` | `skill_code` (PK), `class_code` (FK), libellé, niveau requis, type de ciblage | Compétence proposée par une classe. |
| `HERO_SKILL` | `hero_id` + `skill_code` (PK/FK), `unlocked_at`, `is_active` | Association entre un héros et une compétence maîtrisée. |

Un `PLAYER` peut posséder zéro à plusieurs `HERO`, tandis qu'un héros appartient à un seul joueur. Le MCD de référence limite fonctionnellement ce nombre à dix. Chaque héros appartient à une classe ; une classe propose une ou plusieurs compétences. `HERO_SKILL` rend explicite la relation N-N `masters`, qui porte les attributs `unlocked_at` et `is_active`.

La suppression d'un héros est logique (`est_supprime`, `supprime_le`) et ne doit être possible que lorsqu'il n'est plus référencé par une partie active.

## 2. Idempotence des commandes

| Entité | Attributs principaux | Rôle |
| --- | --- | --- |
| `IDEMPOTENCY_KEY` | `key` (PK), `player_id` (FK), `scope`, `request_fingerprint`, `produced_resource_id`, `expires_at` | Mémorise le résultat d'une commande rejouée. |

Un joueur émet zéro à plusieurs clés. Une commande importante — création de session, jonction, démarrage ou abandon — utilise une clé stable. Un retry avec la même portée et la même empreinte de requête renvoie la ressource ou l'état déjà produit ; une même clé avec une charge différente est refusée.

La contrainte à concrétiser est l'unicité de la clé dans son périmètre, par exemple `UNIQUE(player_id, scope, key)`, ainsi qu'une conservation au moins jusqu'à `expires_at`.

## 3. Matchmaking et groupe

| Entité | Attributs principaux | Rôle |
| --- | --- | --- |
| `MATCHMAKING_QUEUE_ENTRY` | `queue_entry_id` (PK), `player_id` (FK), `hero_id` (FK), état, niveau NG+ à l'entrée, dates | Demande de matchmaking d'un joueur avec le héros mobilisé. |
| `MATCHMAKING_GROUP` | `group_id` (PK), état, NG+ minimal/maximal, date de formation | Groupe issu de la file. |

Un joueur peut rejoindre plusieurs entrées au cours de son historique, mais une entrée appartient à un seul joueur et mobilise un seul héros. Un groupe rassemble les entrées compatibles ; la règle montrée sur le MCD est de **deux à quatre entrées actives** par groupe. Cette borne ne peut pas être exprimée directement par un lien Mermaid : elle doit être contrôlée transactionnellement lors de la formation du groupe.

Le diagramme emploie une relation facultative entre `MATCHMAKING_GROUP` et `GAME_SESSION` (`results_in`) : une entrée peut ne jamais former de groupe, et un groupe peut ne jamais être démarré. Une fois une session créée, le roster est verrouillé.

## 4. GameSession et historique de ses états

| Entité | Attributs principaux | Rôle |
| --- | --- | --- |
| `GAME_SESSION` | `session_id` (PK), état, mode, `dungeon_run_id`, seed, dates et raison de terminaison | Partie gérée par Player. |
| `GAME_SESSION_MEMBER` | `session_id` + `hero_id` (PK/FK), état participant, dates d'entrée/sortie | Participation d'un héros à une session. |
| `GAME_SESSION_TRANSITION` | `transition_id` (PK), `session_id` (FK), état source/cible, cause, acteur, date | Audit des transitions de session. |

Une `GAME_SESSION` accueille un ou plusieurs héros via `GAME_SESSION_MEMBER`. Cette entité associative conserve l'état du participant et évite de rattacher un héros directement à une unique session : un héros peut participer à plusieurs sessions au fil du temps.

Chaque session historise une ou plusieurs `GAME_SESSION_TRANSITION`. Une transition enregistre qui a provoqué le changement, sa cause et les états avant/après ; elle forme l'audit du cycle de vie : création, lobby, démarrage, fin et fermeture.

`dungeon_run_id` est l'identifiant du `DungeonRun` détenu par Dungeon. Il est donc une valeur externe, **sans FK locale**. Il est renseigné après le succès de `CreateDungeonRun`, appel gRPC idempotent envoyé par Player à Dungeon.

Lorsque Dungeon publie `dungeon.dungeon-run-ended.v1` via RabbitMQ, Player termine la `GAME_SESSION`, crée l'historique correspondant et diffuse `SessionStateChanged`. Cette clôture est éventuellement cohérente : elle peut intervenir peu après la fin du donjon.

## Corrections par rapport au schéma dessiné

Le diagramme d'origine fait figurer dans `JOUEUR` l'email, un mot de passe haché, et l'entité `CREDENTIAL` avec un jeton haché. Ces éléments ne sont pas repris dans le MCD final : l'authentification technique, les jetons et leur validation appartiennent à **Keycloak** et **Traefik**, pas à Player. Player reçoit des claims de confiance (`UserId`, `Username`, rôles et `CorrelationId`) et applique ensuite l'autorisation métier sur ses propres ressources.

De même, `id_run_donjon` n'est pas une FK vers une table `DungeonRun` locale : Dungeon est le propriétaire de cette ressource. Cette séparation évite toute lecture ou écriture de la base d'un autre microservice.

## Règles métier à imposer dans les cas d'usage

- Un compte possède au maximum dix héros non supprimés.
- Un héros ne peut être supprimé pendant une session ou un run actif qui le référence.
- Une entrée de file doit utiliser un héros appartenant au joueur demandeur et non supprimé.
- Un groupe en cours de formation contient de deux à quatre entrées actives ; chaque entrée ne rejoint qu'un groupe à la fois.
- Seul le créateur autorisé peut démarrer ou abandonner la session ; le roster est immuable après son verrouillage.
- Toute commande de changement d'état est idempotente et crée au plus une transition effective.
- La fin reçue depuis Dungeon doit être dédupliquée : les redélivrances RabbitMQ ne créent ni seconde clôture ni transition supplémentaire.

## Limites du MCD

Ce document est un modèle conceptuel et un guide de persistance ; il ne fige pas les migrations SQL, la liste exhaustive des états ni le format des contrats gRPC ou RabbitMQ. Les contrôles multi-lignes — taille du groupe, propriété du héros, verrouillage du roster et déduplication concurrente — nécessitent des transactions, contraintes uniques ciblées et verrous applicatifs adaptés.
