# Scripts Review (`Assets/ExponentialGrow/Scripts`)

## Scope
Reviewed gameplay scripts under:
- Core systems (`GameManager`, `Player`, `PlayerInputs`)
- Combat (`CombatSystem`, enemy interactions)
- Combo/effect flow (`ComboManager`, `EffectManager`)

## Fixes Applied
1. Added event unsubscription in lifecycle teardown (`OnDestroy`) for systems subscribing to static events.
   - `CombatSystem`
   - `ComboManager`
   - `EffectManager`
   - `Player`

2. Prevented a null-reference crash in `CombatSystem.ResetCombat()` by returning early when there is no current target.

3. Hardened `Player.OnTakeDamage(...)` logging to avoid null-reference access when `sender` is not a `BaseEnemy`.

## Remaining Recommendations
- Consider centralizing static event ownership and explicit reset on scene unload to avoid stale delegates in play mode iterations.
- Remove empty `Start`/`Update` methods where unnecessary to reduce script noise.
- Normalize naming for discoverability (`Entitys`/`Enemys`/`Reource...`) in a future refactor.
