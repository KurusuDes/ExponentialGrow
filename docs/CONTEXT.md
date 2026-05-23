# ExponentialGrow — Contexto del Proyecto

> Documentación viva del proyecto. Última actualización: 2026-05-23.
> Nombre interno detectado en menús de editor: **"DungeonDiscotecDisaster"**.

---

## 1. Resumen general

**ExponentialGrow** es un **roguelike de mazmorra con combate por turnos** hecho en **Unity 6 (6000.3.8f1)** con render pipeline **URP 17.3.0**. La mecánica diferenciadora es un **sistema de combos de teclas direccionales** (↑ ↓ ← →) que se introducen contra un temporizador y se resuelven contra un **árbol de combos** definido en ScriptableObjects. Cada combo dispara un **efecto** con un **género musical** (Rock, Pop, Salsa, Electronic, etc.) que interactúa con la **raza** del enemigo (Goblin, Vampire, Ogre, Fairy, Orcs, Undead) mediante tablas de afinidad/debilidad.

El nombre del repo es `ExponentialGrow` pero el menú interno de Unity (`Tools/DungeonDiscotecDisaster/Combo Graph Window`) sugiere que el título de marketing del juego es **"Dungeon Discotec Disaster"**.

---

## 2. Stack técnico

| Categoría | Tecnología |
|-----------|-----------|
| Motor | Unity **6000.3.8f1** |
| Render | Universal Render Pipeline 17.3.0 + Shader Graph |
| Input | Unity Input System 1.18.0 (`InputSystem_Actions.inputactions`) |
| Tilemaps | 2D Tilemap + Extras 6.0.1 |
| Nav | AI Navigation 2.0.10 |
| 3D modelado | ProBuilder 6.0.9 |
| Postproceso | Post Processing 3.5.1 |
| VCS | Plastic SCM (Unity Collab Proxy) + Git |

### Assets de terceros (Asset Store)

| Paquete | Uso en el proyecto |
|---------|-------------------|
| **Sirenix Odin Inspector** | `[FoldoutGroup]`, `[Button]`, `[InlineEditor]`, `SerializedScriptableObject`, `SerializedMonoBehaviour`, serialización de `Dictionary<>` en SOs |
| **MoreMountains Feel** | `MMF_Player` para feedbacks visuales/sonoros del enemigo (spawn, hit, prepare, attack, defeated) |
| **MoreMountains NiceVibrations** | Vibraciones para feedback háptico |
| **DamageNumbersPro** | Popups de daño y combo (`DamageNumber.Spawn`, `SpawnGUI`) |
| **DOTween** (DG.Tweening) | Animación procedural (usado en `AttackDirectionVisualController`) |
| **HotReload (Singularity Group)** | Iteración rápida en editor |
| **Console Pro** | Reemplazo del console de Unity |
| **ParticleImage** | Partículas en UI |
| **Hovl Studio** | Pack de VFX |
| **SimpleToon / PolygonDungeon / BitDungeon / AssetKits** | Arte 3D estilizado para el dungeon |

---

## 3. Estructura de carpetas

```
ExponentialGrow/
├── Assets/
│   └── ExponentialGrow/              # Carpeta propia del proyecto
│       ├── Animations/               # Anims de flechas direccionales (ArrowUp/Down/Left/Right + Interact)
│       ├── Prefab/                   # BaseEnemy, ComboTypeVisual, VisualArrow, UI/, Enemy/, Scenary/
│       ├── Scenes/                   # GameScene.unity (escena principal)
│       ├── ScriptableObjects/        # Instancias: KeyCapDatabase, CombatSystem/, ComboData/, EffectData/, EntityActtacks/, UI/, ArrowUp/Down/Left/Right.asset
│       ├── Scripts/                  # Código del juego (ver §4)
│       └── Sprites/                  # Sprites 2D (UI y enemigos)
├── Packages/manifest.json            # Dependencias Unity
├── ProjectSettings/                  # Unity project settings
└── docs/CONTEXT.md                   # Este documento
```

---

## 4. Arquitectura de Scripts

### 4.1 Vista global

El proyecto sigue un patrón **event-driven con Singletons**. Los managers se comunican vía `static Action` (event bus implícito), y los datos viven en **ScriptableObjects** consumidos por los managers.

```
PlayerInputs ──OnAttack──▶ ComboManager ──OnComboTypeTrigger──▶ EffectManager
                                │                                      │
                                │                                      ▼
                                └─OnComboPredictTrigger──▶ UI    OnCastEffect
                                                                       │
                                                                       ▼
                                                                    Player
                                                                       │
                                                              OnEffectCasted
                                                                       │
                                                                       ▼
                                                                 CombatSystem
                                                                       │
                                                                       ▼
                                                                BaseEnemy.OnTakeDamage
                                                                       │
                                                                       ▼ (al morir)
                                                                OnEnemyDefeated
                                                                       │
                                                                       ▼
                                                              CombatSystem.SpawnNextEnemy
```

### 4.2 Núcleo y entrada

| Script | Carpeta | Rol |
|--------|---------|-----|
| `GameManager` | `Scripts/` | **Singleton raíz**. Mantiene referencia a `Player`, `CombatSystem`, `KeyCapDatabase`, `EnemyDatabaseSO` y `SeedRandom`. Expone `GetRandomEnemy()` y `ActiveEnemy(EnemySO)`. |
| `GameManagerUI` | `Scripts/UI/` | Singleton paralelo para UI. Tiene `ReourceUIDatabaseSO` (sprites de keycaps y combo types). |
| `Player` | `Scripts/` | `MonoBehaviour` que implementa `IDamageable`. Mantiene lista de `IAttackModifier`. Se suscribe a `EffectManager.OnCastEffect`, aplica modificadores y emite `CombatSystem.OnEffectCasted`. |
| `PlayerInputs` | `Scripts/` | Envuelve `InputSystem_Actions` (acción `PlayerDungeon`). Dispara `OnAttack(KeyCapType)` y `OnInteraction` como `static Action`. |
| `SeedRandom` | `Scripts/Core/` | Tres `System.Random` deterministas: `EnemyGenerator`, `LootGenerator`, `RoomGenerator`. Semilla por defecto = `DateTime.Now.Ticks`. |
| `Interfaces` | `Scripts/` | `IAttackModifier.ApplyModifier(Effect)` e `IDamageable.OnTakeDamage(int, GenreType, IDamageable)`. |
| `Enum` | `Scripts/` | Enumeraciones globales del dominio (ver §5). |

### 4.3 Sistema de Combos

| Script | Rol |
|--------|-----|
| `ComboManager` (Singleton) | Recibe inputs vía `PlayerInputs.OnAttack`, los acumula en una `Queue<KeyCapType>` con timeout `maxInputDelay`. Al expirar, dispara `OnComboTrigger` y resuelve el combo contra el `ComboDatabaseSO`. Emite `OnComboTypeTrigger(ComboName)`, `OnComboPredictTrigger(current, next)`, `OnComboEnded`. |
| `ComboNodeSO` | Nodo del árbol de combos. Diccionario `Dictionary<KeyCapType, ComboNodeSO> paths` + `ComboName Value` + `ComboType Type`. Usa `SerializedScriptableObject` (Odin) para serializar el dict. |
| `ComboDatabaseSO` | Raíz del árbol de combos. Diccionario raíz `Dictionary<KeyCapType, ComboNodeSO> comboData`. Métodos: `GetComboType(Queue)`, `GetComboNode(Queue)`, `GetAllPossibleCombos(root)`, `GetFirstLevelOptions(node)`, `GetFirstLevelBaseData(node)`. |
| `ComboGraphWindow` (`Combo/ComboVisualizer/`) | **Ventana de editor custom** (`Tools/DungeonDiscotecDisaster/Combo Graph Window`). Renderiza el árbol de combos con bezier curves, soporta pan (botón medio), zoom (rueda), drag de nodos y persistencia de layout vía `EditorPrefs`. |
| `ComboGraphViewNode` | Vista de un nodo en el grafo: header coloreado por `ComboType`, listado de paths, selección y arrastre. |

### 4.4 Sistema de Efectos

| Script | Rol |
|--------|-----|
| `EffectManager` | Escucha `ComboManager.OnComboTypeTrigger`, busca el `EffectSO` correspondiente en `EffectDatabaseSO`, construye un `Effect` runtime y dispara `OnCastEffect`. También muestra el popup visual con `DamageNumbersPro`. |
| `EffectSO` | Datos serializados del efecto: `GenreType`, `BaseDamage`, `BaseDefense`. ⚠️ Marcado con TODO de unificar con `EntityActionSO`. |
| `Effect` | Instancia runtime construida desde `EffectSO + ComboName`. `ActiveEffect(modifiers)` aplica todos los `IAttackModifier` antes de retornar. |
| `EffectDatabaseSO` | `Dictionary<ComboName, EffectSO>`. Fallback a `ComboName.Miss` si no encuentra. |

### 4.5 Sistema de Combate

| Script | Rol |
|--------|-----|
| `CombatSystem` | Spawn/destruye enemigos, mantiene `CurrentTarget`, escucha `OnEffectCasted` para aplicar daño al target, escucha `OnEnemyDefeated` para premiar y respawnear con 2 segundos de delay. Eventos públicos: `OnEnemyDefeated(Reward)`, `OnEffectCasted(Effect, IDamageable)`, `OnInstantiateEnemy(BaseEnemy)`, `OnCreateEnemy`. |
| `BaseEnemy` | `MonoBehaviour` + `IDamageable`. Máquina de estados `EnemyState`: `Idle → PreparingAttack → ChargingAttack → AboutToAttack → Attacking → Idle` (loop). Mantiene `Queue<EntityAction>` rotatoria. Cada estado dispara un `MMF_Player` de Feel. Genera eventos: `OnSpawn`, `OnAttack`, `OnGetHit`, `OnDefeated`, `OnActionProgress`, `OnActionStarted`. |
| `EnemySO` | Datos: `entityName`, `race`, `chanceToInvoke (1-100)`, `hitpoints (1-10)`, `actionSpeed (0-20)`, lista de `EntityActionSO`, `Reward`, `icon`. Calcula `entityValue` auto en `OnValidate`. |
| `EnemysDatabaseSO` | `Dictionary<RarityType, List<EnemySO>>` + `RarityOddsTable`. `GetEnemy(seed)` rolla rareza, luego enemigo dentro de la rareza. |
| `RarityOddsTable` | Tabla `List<RarityOdds>` con peso 0-100. `AutoBalance()` reescala a 100. `SetTable()` setea defaults: Common 35 / Uncommon 30 / Rare 20 / Epic 10 / Legendary 5. |
| `EntityAction` | Instancia runtime de `EntityActionSO`. `ApplyEffect(target, sender)` llama a `target.OnTakeDamage(damage, genre, sender)`. |
| `EntityActionSO` | Datos: `Vfx`, `AttackType`, `StanceType`, `GenreType`, `baseDamage`, `baseDefense`. ⚠️ Marcado con TODO de unificar con `EffectSO`. |
| `Reward` | `gold (0-100)`, `experiencePoints (0-100)`, `List<GameObject> items`. |
| `HealthBarUI` | Sistema de corazones (full/half/empty) instanciados como hijos. `Set(hp, maxHp)` reconstruye desde cero. |
| `EntityUI` / `EnemyEntityGUI` | **Duplicados** (`EntityUI.cs` y `EnemyEntityGUI.cs` son virtualmente idénticos). Bindean a `CombatSystem.OnInstantiateEnemy` y pintan slider de progreso, healthbar y popups de ataque del enemigo. |
| `HealthBarView` | ⚠️ Esqueleto. Define enums `HeartVisualLvl`, `HeartSpaces` y diccionarios de materiales/offsets pero sin lógica. |
| `Modifier` / `ModifierSO` | `Modifier` implementa `IAttackModifier` pero `ApplyModifier` está vacío. `ModifierType` enum solo tiene `None`. |

### 4.6 Sistema de Mazmorras (Dungeon)

| Script | Rol |
|--------|-----|
| `RoomManager` | ⚠️ Singleton vacío. Solo tiene un `[Button] TestRng()` que imprime un número aleatorio. |
| `RoomNode` | `roomType`, `roomState`, `connectedRooms`, `List<BaseEnemy>`, `StartPosition`. Eventos `OnPlayerEnterRoom` (→ `LoadLevel`) y `OnPlayerExitRoom` (→ `HideLevel`). Las funciones `LoadLevel`/`HideLevel` solo cambian el state, sin lógica de carga real. |
| `Door` | ⚠️ Vacío (solo `Start`/`Update` por defecto). |

**Estado:** el sistema de mazmorras es el componente menos desarrollado del proyecto.

### 4.7 UI

| Script | Rol |
|--------|-----|
| `ComboPredictUI` | Escucha `OnComboPredictTrigger`. Dibuja la fila del combo actual (opaco) y los siguientes posibles (semitransparente) instanciando `ArrowKeyUI`. |
| `ArrowKeyUI` | Renderiza una tecla: sprite de flecha + icono de combo type. Acepta `SpriteType.Full` (alfa 1) o `Half` (alfa 0.7). |
| `ReourceUIDatabaseSO` | Diccionarios `Dictionary<KeyCapType, Sprite>` y `Dictionary<ComboType, Sprite>` para resolver sprites desde la UI. |
| `GameManagerUI` | Singleton de UI con referencia al SO de arriba. |
| `PlayerUI` | ⚠️ Vacío. |

### 4.8 Herramientas de Editor

| Script | Rol |
|--------|-----|
| `ModularBrushWindow` (`Sowtank Tools/Modular Brush`) | **Pincel de prefabs para nivel design**. Cuelga de `SceneView.duringSceneGui`. Soporta snap a grid, raycast a superficies o plano fijo, auto-stacking sobre `LayerMask`, rotación con CTRL+rueda (step configurable), categorías (Floors1/2, Walls1/2, Misc1/2, Custom), preview translúcido con material override, undo nativo. |
| `ComboGraphWindow` (`Tools/DungeonDiscotecDisaster/Combo Graph Window`) | Ver §4.3. |

### 4.9 Misceláneo

| Script | Rol |
|--------|-----|
| `KeyCapSO` | Datos de una tecla: `KeyCapType` + `GameObject prefab` (visual 3D de la tecla). |
| `KeyCapDatabaseSO` | `Dictionary<KeyCapType, KeyCapSO>`. |
| `AttackDirectionVisualController` | Componente del prefab visual de tecla. En `Set(cap)` reproduce la animación correspondiente y se autodestruye al terminar. |

---

## 5. Enumeraciones del dominio

```csharp
KeyCapType  : Up, Down, Left, Right, Item
ComboType   : None, Offensive, Defensive, Utility, Control
ComboName   : Miss, Basic, HardRockDrift, AltoAmperage, SmoothDrift,
              SourRockDrift, ChargeFront, Parry, DodgeLeft, DodgeRight
GenreType   : None, OutOfTune, Clasic, Rock, HipHop, Salsa, Electronic,
              Folk, Metal, Pop (flags) + All
RacesType   : None, Goblin, Undead, Ogre, Vampire, Fairy, Orcs
AttackType  : None, Bludgeoning, Piercing, Slashing
StanceType  : None, Steady, Unstable, Charger
EnemyState  : Idle, PreparingAttack, ChargingAttack, AboutToAttack,
              Attacking, Defeated
GameState   : None, Explore, Combat                    (definido, no usado)
RarityType  : Common, Uncommon, Rare, Epic, Legendary
RoomType    : None, Start, Combat, Treasure, Boss, Shop, Exit
RoomState   : Unvisited, Visited, Cleared
```

### Tabla de género ↔ raza (según comentarios en `Enum.cs`)

| Género | Afinidad (raza fuerte contra) | Debilidad |
|--------|-------------------------------|-----------|
| Clasic | Vampire | Pop |
| Rock | Ogre | Clasic |
| HipHop | Orcs | Rock |
| Salsa | Fairy | Metal |
| Electronic | Undead | Folk |
| Folk | Goblin | Electronic |
| Metal | Ogre | Pop |
| Pop | Fairy | Metal |

Combinaciones documentadas: `Clasic = Folk + Electronic`, `Salsa = Folk + HipHop`, `Metal = Rock + Electronic`, `Pop = HipHop + Electronic`.

---

## 6. ScriptableObjects existentes

Carpeta `Assets/ExponentialGrow/ScriptableObjects/`:

- `ArrowUp.asset`, `ArrowDown.asset`, `ArrowLeft.asset`, `ArrowRight.asset` — instancias de `KeyCapSO`
- `KeyCapDatabase.asset` — instancia única de `KeyCapDatabaseSO`
- `CombatSystem/` — datos de CombatSystem (DBs de enemigos, etc.)
- `ComboData/` — árbol de combos (instancias de `ComboNodeSO` y `ComboDatabaseSO`)
- `EffectData/` — `EffectSO` indexados por `ComboName`
- `EntityActtacks/` — `EntityActionSO` que usan los enemigos *(typo "Actacks")*
- `UI/` — `ReourceUIDatabaseSO` *(typo "Reource")*

---

## 7. Estado actual

### ✅ Sistemas funcionales

- **GameManager / SeedRandom**: singleton estable, RNG determinista por categoría.
- **Combo system**: árbol funcional, predicción de paths posibles, ventana de editor profesional con pan/zoom/persistencia.
- **Effect system**: bridge entre combo y daño funciona end-to-end.
- **CombatSystem**: bucle completo spawn → combate → reward → respawn.
- **BaseEnemy**: state machine con feedbacks integrados, queue rotatoria de acciones.
- **ComboPredictUI**: muestra combo actual + siguientes opciones en tiempo real.
- **ModularBrushWindow**: pincel de prefabs production-ready (snap, stacking, rotación, undo, preview).

### ⚠️ Parcialmente implementado

- **Player**: implementa `IDamageable` pero `OnTakeDamage` solo imprime; sin HP propios.
- **Modifiers**: estructura definida (`IAttackModifier`, `Modifier`, `ModifierSO`) pero `ApplyModifier` está vacío y `ModifierType` solo tiene `None`.
- **BaseEnemy.ActionResolver**: el switch sobre `StanceType` está vacío para `Steady`/`Unstable`.
- **HealthBarView**: esqueleto con enums y diccionarios, sin lógica.
- **Reward.items**: lista existe pero `RewardMechanism` solo imprime gold/exp, no procesa items.

### ❌ Esqueleto / no implementado

- **RoomManager / Door / RoomNode**: marcos casi vacíos. No hay generación procedural ni navegación entre cuartos.
- **PlayerUI**: vacío.
- **GameState** (Explore/Combat): enum existente pero no se usa en ningún manager.
- **`RoomType` Treasure/Shop/Boss**: sin implementación.

### 🔁 Duplicación detectada

- **`EntityUI.cs` vs `EnemyEntityGUI.cs`**: archivos virtualmente idénticos (mismo código, diferente nombre de clase).
- **`EffectSO` vs `EntityActionSO`**: contienen los mismos campos (`GenreType`, `baseDamage`, `baseDefense`). Marcado con TODO en ambos archivos: *"Considerar unificar el EffectSO y el EntityActionSO en uno solo hacen basicamente lo mismo"*.

### 🐛 Issues y TODOs explícitos en código

| Ubicación | Comentario |
|-----------|-----------|
| `EffectSO.cs:5`, `EntityActionSO.cs:4`, `CombatSystem.cs:73` | "Considerar unificar el EffectSO y el EntityActionSO" |
| `Player.cs:11` | "los modificadores deberian estar en el player?" |
| `BaseEnemy.cs:89` | "hook this to an event that pass the turn once the player end his combo or fail his combo" |
| `BaseEnemy.cs:285` | "llamar a clase que lea el daño y el typo y o reste daño o duplique el daño, Idea: las habilidades tienen cd si spameas una habilidad pierde el bono de genero" |
| `EntityAction.cs:28` | "aqui se deberia llamar la raza el tipo y calculo de daño / algo como una clase estatica damagecalculator" |
| `HealthBarUI.cs:10` | "un solo prefab HeartUI que maneje 3 estados y animaciones" |
| `EnemysDatabaseSO.cs:24` | "Completar" en sobrecarga `GetEnemy(int)` |

---

## 8. Mejoras propuestas (no priorizadas)

### Arquitectura / refactors

1. **`DamageCalculator` estático**: extraer el cálculo de daño (modificadores + relación género/raza + stance + ¿cooldown anti-spam?) fuera de `BaseEnemy.OnTakeDamage`. Resolvería 3 TODOs.
2. **Unificar `EffectSO` y `EntityActionSO`** en un `CombatActionSO` único; ambos comparten esquema.
3. **Eliminar el duplicado `EntityUI.cs` ↔ `EnemyEntityGUI.cs`**: dejar uno solo y actualizar prefabs.
4. **Reemplazar las `static Action`** dispersas por un `EventBus` centralizado (`GameEvents`) para auditabilidad y evitar suscripciones huérfanas.
5. **Aprovechar `GameState`** ya definido: introducir un `GameStateMachine` que coordine explore vs combat (hoy el flujo es implícito).

### Gameplay pendientes

6. **Sistema de mazmorras**: implementar generación de rooms (RoomManager debería usar `SeedRandom.RoomGenerator`), navegación por `Door` y enganchar `RoomNode.OnPlayerEnterRoom` con `CombatSystem.SpawnEnemy`.
7. **HP del Player**: hoy `Player.OnTakeDamage` solo imprime. Falta vida, defensa, shield (`Effect.Shield` ya existe pero no se aplica).
8. **Loot / items**: `Reward.items` existe pero `CombatSystem.RewardMechanism` la ignora. Conectar con inventario del Player.
9. **Tabla género↔raza ejecutable**: documentada en `Enum.cs` como comentario, pero ningún script la consume. Materializarla en un `RaceGenreAffinitySO`.
10. **Tipos de Room**: `Treasure`, `Shop`, `Boss`, `Exit` están enumerados pero sin lógica. Diseñar interacciones.
11. **Stances funcionales**: implementar comportamiento real de `Steady`/`Unstable`/`Charger` en `BaseEnemy.ActionResolver`.

### Polish técnico

12. **Renombrar typos** de archivos/carpetas: `ReourceUIDatabaseSO` → `ResourceUIDatabaseSO`, `EntityActtacks/` → `EntityAttacks/`.
13. **`SeedRandom`** usa la misma seed para los tres generators — debería ser `seed`, `seed + 1`, `seed + 2` (o derivarse con hashing) para evitar correlaciones.
14. **`ShowVisualAttack`** en `ComboManager` está comentado en `Awake` (línea 41). Decidir si removerlo del todo o reactivarlo.
15. **`Debug.WriteLine`** en `SeedRandom.Initialize` no se muestra en consola de Unity — usar `Debug.Log`.
16. Borrar bloques de código comentado obsoletos en `ComboPredictUI` y `BaseEnemy`.
17. **Sustituir `print(...)`** por `Debug.Log` (cosmético, pero `print` es solo accesible desde MonoBehaviour).

---

## 9. Objetivos del proyecto

> **TODO — definir con el usuario.**
> Aún no hay objetivos formales declarados. Algunas hipótesis a confirmar:
>
> - ¿Demo jugable de un piso completo de dungeon?
> - ¿Vertical slice (1 raza × N géneros) para showcase?
> - ¿Build para itch.io / Steam?
> - ¿Multiplayer? (`com.unity.multiplayer.center` está en el manifest pero sin uso visible).
>
> **Próximo paso:** definir objetivos en esta sección y enlazarlos a las mejoras de §8.

---

## 10. Cómo orientarse rápido

- **Entrada del juego**: `GameScene.unity` → `GameManager` en escena → spawn de enemigos vía botón `[Button] CreateEnemyTest()` en `CombatSystem`.
- **Editar el árbol de combos**: menú `Tools > DungeonDiscotecDisaster > Combo Graph Window`, asignar `ComboDatabaseSO` → "Generate Graph".
- **Editar nivel**: menú `Sowtank Tools > Modular Brush`, activar brush, seleccionar categoría/prefab y clic en SceneView.
- **Crear enemigo**: `Create > Scriptable Objects > EnemySO`, asignar actions (`ActionSO`) y registrarlo en el `EnemyDatabaseSO`.
- **Crear combo nuevo**:
  1. Añadir entrada a `ComboName` enum.
  2. Crear `ComboNodeSO` con su `Value` y `Type`.
  3. Enganchar el nodo al árbol en `ComboDatabaseSO` (vía Combo Graph Window).
  4. Crear `EffectSO` y registrarlo en `EffectDatabaseSO` con la nueva `ComboName`.

---

## 11. Branches y workflow git

- `main` — rama estable.
- `claude/epic-mendel-eb44N` — rama designada para desarrollo asistido por Claude.

Historial reciente:

```
71822b6 Combo graph
ccf7acb deco
3633790 Pusheito
4ae91c7 Implementation of the new enemy
ee0be5c ModuleTest
7813d65 Merge pull request #2 from KurusuDes/CodexTest
d99351b Merge pull request #1 from KurusuDes/codex/review-scripts-in-assets/exponentialgrow
e0d81c2 Review ExponentialGrow scripts and harden event/combat safety
1f5eb0c Initial commit
```
