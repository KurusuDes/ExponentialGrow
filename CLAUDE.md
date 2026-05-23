# CLAUDE.md — ExponentialGrow

> Resumen ejecutivo del proyecto para Claude. Documento extenso en `docs/CONTEXT.md`.

## Qué es

Roguelike de mazmorras en **Unity 6 (6000.3.8f1) + URP** con combate por turnos basado en **combos de teclas direccionales** (↑ ↓ ← →). Cada combo dispara un **efecto** con un **género musical** (Rock, Pop, Salsa, Electronic, etc.) que interactúa con la **raza** del enemigo (Goblin, Vampire, Ogre, Fairy, Orcs, Undead).

Nombre interno detectado en menús de editor: **"DungeonDiscotecDisaster"**.

## Arquitectura en una vista

```
PlayerInputs → ComboManager → EffectManager → Player → CombatSystem → BaseEnemy
                     │                                                    │
                     └──────────► ComboPredictUI (preview de teclas)      │
                                                                          ▼
                                                                  Reward → respawn
```

- **Event-driven** vía `static Action` (no hay event bus central — TODO).
- **Datos en ScriptableObjects** (Odin `SerializedScriptableObject` para serializar `Dictionary<>`).
- Singletons: `GameManager`, `GameManagerUI`, `ComboManager`, `RoomManager`.

## Stack clave

| Categoría | Tech |
|-----------|------|
| Motor | Unity 6000.3.8f1, URP 17.3.0, Input System 1.18 |
| Editor | **Sirenix Odin Inspector** (omnipresente) |
| Feedback | **MoreMountains Feel** (MMF_Player), NiceVibrations, **DamageNumbersPro**, DOTween |
| Iteración | Singularity HotReload, Console Pro |

## Carpetas a recordar

```
Assets/ExponentialGrow/
├── Scripts/
│   ├── (root)        GameManager, Player, PlayerInputs, Enum, Interfaces, Modifier(SO), KeyCap(SO/DatabaseSO), RarityOddsTable, AttackDirectionVisualController
│   ├── Combat/       CombatSystem, BaseEnemy parts (EntityUI, EnemyEntityGUI⚠duplicado, HealthBarUI, HealthBarView⚠, EnemysDatabaseSO)
│   ├── Combo/        ComboManager, ComboNodeSO, ComboDatabaseSO, ComboVisualizer/* (editor window)
│   ├── Core/         SeedRandom
│   ├── Dungeon/      RoomManager⚠, RoomNode⚠, Door⚠ (TODO casi vacíos)
│   ├── EditorTools/  ModularBrushWindow
│   ├── Effects/      EffectManager, EffectSO, Effect (runtime), EffectDatabaseSO
│   ├── Entitys/      BaseEnemy, EnemySO, EntityAction, EntityActionSO, Reward
│   ├── Player/       PlayerUI⚠ (vacío)
│   └── UI/           ComboPredictUI, ArrowKeyUI, ReourceUIDatabaseSO (typo), GameManagerUI
├── Scenes/GameScene.unity
├── ScriptableObjects/  (data: ArrowUp/Down/Left/Right, KeyCapDatabase, CombatSystem/, ComboData/, EffectData/, EntityActtacks/ (typo), UI/)
└── Prefab/             BaseEnemy, ComboTypeVisual, VisualArrow, Enemy/, Scenary/, UI/
```

## Enumeraciones del dominio

- `KeyCapType`: Up, Down, Left, Right, Item
- `ComboType`: None, Offensive, Defensive, Utility, Control
- `ComboName`: Miss, Basic, HardRockDrift, AltoAmperage, SmoothDrift, SourRockDrift, ChargeFront, Parry, DodgeLeft, DodgeRight
- `GenreType` (flags): None, OutOfTune, Clasic, Rock, HipHop, Salsa, Electronic, Folk, Metal, Pop
- `RacesType`: None, Goblin, Undead, Ogre, Vampire, Fairy, Orcs
- `EnemyState`: Idle → PreparingAttack → ChargingAttack → AboutToAttack → Attacking → Idle
- `RarityType`: Common, Uncommon, Rare, Epic, Legendary
- `RoomType`: None, Start, Combat, Treasure, Boss, Shop, Exit
- `GameState`: None, Explore, Combat **(definido, sin usar)**

## Estado actual

| Sistema | Estado |
|---------|--------|
| GameManager / SeedRandom | ✅ Funcional |
| Combo system (árbol + editor visual) | ✅ Funcional |
| Effect system | ✅ Funcional |
| CombatSystem (loop spawn→combate→reward) | ✅ Funcional |
| BaseEnemy state machine + Feel feedbacks | ✅ Funcional |
| ComboPredictUI | ✅ Funcional |
| ModularBrushWindow (editor) | ✅ Production-ready |
| Player (HP, modificadores) | ⚠️ Parcial — `OnTakeDamage` solo imprime, sin HP |
| Modifiers (`IAttackModifier`) | ⚠️ Estructura sin implementación |
| HealthBarView | ⚠️ Esqueleto |
| Reward.items | ⚠️ Lista existe, no se procesa |
| Stances (Steady/Unstable/Charger) | ⚠️ Switch vacío en `BaseEnemy.ActionResolver` |
| **Dungeon system (RoomManager/Door/RoomNode)** | ❌ Casi vacíos |
| PlayerUI | ❌ Vacío |
| GameState explore vs combat | ❌ Enum existe, no se usa |

## Issues / TODOs en código

- `EffectSO` y `EntityActionSO` son virtualmente iguales — TODO de unificar (`EffectSO.cs:5`, `EntityActionSO.cs:4`, `CombatSystem.cs:73`).
- `EntityUI.cs` ↔ `EnemyEntityGUI.cs` son archivos duplicados.
- `BaseEnemy.OnTakeDamage` necesita `DamageCalculator` estático que aplique género/raza/stance/cooldown (`BaseEnemy.cs:285`, `EntityAction.cs:28`).
- `HealthBarUI` debería usar un prefab unificado con 3 estados (`HealthBarUI.cs:10`).
- `EnemysDatabaseSO.GetEnemy(int)` marcado como "completar" (`EnemysDatabaseSO.cs:24`).
- `SeedRandom` usa la misma semilla para los 3 generators (correlaciones).
- Typos: `ReourceUIDatabaseSO` → `ResourceUIDatabaseSO`, `EntityActtacks/` → `EntityAttacks/`.

## Mejoras propuestas (ver `docs/CONTEXT.md §8` para la lista completa)

1. `DamageCalculator` estático que aplique modificadores + relación género/raza.
2. Unificar `EffectSO` + `EntityActionSO` en `CombatActionSO`.
3. Eliminar duplicado `EntityUI`/`EnemyEntityGUI`.
4. `EventBus` centralizado en lugar de `static Action` dispersas.
5. Implementar generación procedural de mazmorra (usar `SeedRandom.RoomGenerator`).
6. HP/defensa real del Player; aplicar `Effect.Shield`.
7. Procesar `Reward.items` en `CombatSystem.RewardMechanism`.
8. Materializar la tabla género↔raza (hoy solo comentario) en un `RaceGenreAffinitySO`.
9. Implementar Stances `Steady`/`Unstable`/`Charger`.
10. Implementar `RoomType.Treasure`/`Shop`/`Boss`.

## Objetivos del proyecto

> **TODO — pendiente de definir con el usuario.** Ver `docs/CONTEXT.md §9` con hipótesis abiertas (vertical slice, demo jugable, etc.).

## Convenciones del repo

- **Idioma**: comentarios en español, código y nombres en inglés. Mantén la convención al editar.
- **Inspector**: usa atributos Odin (`[FoldoutGroup]`, `[Button]`, `[InlineEditor]`) — ya es el estándar del proyecto.
- **ScriptableObjects con dicts**: heredar de `SerializedScriptableObject` y usar `[OdinSerialize]`.
- **Eventos**: hoy son `static Action` — al añadir nuevos, suscribirse en `Awake`/`OnEnable` y desuscribirse en `OnDestroy`/`OnDisable` (ya hay precedente).
- **Comentarios TODO**: usar `//->` (convención existente en el código del proyecto).

## Cómo orientarse rápido

- Entrada del juego: `GameScene.unity` → `GameManager` → `[Button] CreateEnemyTest()` en `CombatSystem`.
- Editar combos: **Tools > DungeonDiscotecDisaster > Combo Graph Window**.
- Editar niveles: **Sowtank Tools > Modular Brush**.
- Crear combo nuevo: 1) añadir a `ComboName` enum 2) crear `ComboNodeSO` 3) enganchar al árbol 4) crear `EffectSO` y registrar en `EffectDatabaseSO`.

## Branches

- `main` — rama estable.
- `claude/epic-mendel-eb44N` — rama designada para desarrollo asistido (override explícito: usuario indicó push a `main` para esta documentación).

## Documentación en Notion (creada 2026-05-23)

Hub principal: https://www.notion.so/369ac10136a7813598d7dce9b63939e4

| Página | Link |
|--------|------|
| Resumen y Stack Técnico | https://www.notion.so/369ac10136a7810ea962d7760d0d10bd |
| Arquitectura de Scripts | https://www.notion.so/369ac10136a7812a8d8bec9a9ce90a46 |
| Enumeraciones y Tablas de Datos | https://www.notion.so/369ac10136a78166aeeac7986f33ed85 |
| Estado del Proyecto | https://www.notion.so/369ac10136a781dfbeb4c74479c4d148 |
| Mejoras Propuestas — Roadmap | https://www.notion.so/369ac10136a78138a346e68f3b63b7fa |
| Guía Rápida del Desarrollador | https://www.notion.so/369ac10136a7814d9c7dd4ddada78e9f |

> Al actualizar el estado del proyecto, mantener también las páginas de Notion sincronizadas (especialmente "Estado del Proyecto" y "Mejoras Propuestas").

## Historial de sesiones con Claude

### Sesión 2026-05-23
- Leído `CLAUDE.md` y `docs/CONTEXT.md` para obtener contexto completo del proyecto.
- Creado hub de documentación en Notion con 6 sub-páginas (ver tabla arriba).
- Próximos pasos sugeridos: definir objetivos del proyecto (`docs/CONTEXT.md §9`), implementar `DamageCalculator` estático.
