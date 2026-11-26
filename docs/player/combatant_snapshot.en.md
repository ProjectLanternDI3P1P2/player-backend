# Combatant snapshot

`PlayerHeroService.GetCombatantSnapshot` (internal gRPC, `Player.Contracts`) gives Combat
what it needs to put a hero in a fight. Combat calls it once, when the fight opens: the
snapshot is the hero's immutable initial state for that fight.

## Rules

- Unknown hero, or deleted hero: `NOT_FOUND` (a deleted hero can no longer fight; its
  appearance stays readable through `GetHeroAppearance`).
- Invalid or empty `hero_id`: `INVALID_ARGUMENT`.
- Pure read: no write, no outgoing call. The same hero gives the same reply.
- Player does not track hit points during a run: `current_hp = max_hp` and
  `current_mana = max_mana`. Dungeon carries the run's values and sends them to Combat.

## Formulas

Isolated in `Player.Domain.Services.CombatantStatsCalculator`; the balance is expected to change.

| Statistic | Formula |
| --- | --- |
| `max_hp` | `HeroClass.BaseHealth + Endurance × 6` (same as the hero sheet) |
| `max_mana` | `HeroClass.BaseMana + Intelligence × 3` |
| `attack` | `Strength` for warrior, `Intelligence` for mage and shaman |
| `defense` | `Endurance / 2` (rounded down) |
| `speed` | `Agility` |

Values are never negative; `max_hp` is at least 1.

## Abilities

Active hero skills whose `RequiredLevel <= Level`, ordered by code. Each skill carries
`mana_cost` and `power` (`skill.mana_cost`, `skill.power`): `power` is a percentage of the
caster's attack for an offensive ability, or of the target's maximum hit points for a
healing one.

| Skill | Mana cost | Power |
| --- | --- | --- |
| `warrior-strike` | 4 | 150 |
| `mage-bolt` | 6 | 170 |
| `shaman-totem` | 5 | 140 |

## Appearance

`appearance` repeats the `GetHeroAppearance` reply: cosmetic codes only, never an image.
