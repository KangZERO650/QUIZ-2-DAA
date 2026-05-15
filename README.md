# Analysis Report

| Name              | NRP        |
|-------------------|------------|
| Raziq Danish Safaraz          | 5025241258      |
| Aminnudin Wijaya          | 5025241242      |
| Herdian Tri Wardhana           | 5025241229      |

`Raziq`: designed the user interface and developed the main structure <br>
`Amin`: handled backend logic and implemented Djikstra Algorithm <br>
`Herdian` : Perform analysis, testing and write the final report <br>

---

## Table of Contents

- [1. Design](#1-design)
  - [1.1 Game Description](#11-game-description)
  - [1.2 Game Space Design](#12-game-space-design)
  - [1.3 Entity Elements](#13-entity-elements)
  - [1.4 Algorithm Paradigm](#14-algorithm-paradigm)
- [2. Implementation](#2-implementation)
  - [2.1 Architecture & Project Structure](#21-architecture--project-structure)
  - [2.2 Core Data Structures](#22-core-data-structures)
  - [2.3 AI Mechanics — GhostAI](#23-ai-mechanics--ghostai)
  - [2.4 Player Mechanics — PlayerController](#24-player-mechanics--playercontroller)
  - [2.5 Movement System — CubeRoller](#25-movement-system--cuberoller)
  - [2.6 Power-Up & Collectible System](#26-power-up--collectible-system)
  - [2.7 Portal & Teleportation System](#27-portal--teleportation-system)
  - [2.8 Scene & UI Management](#28-scene--ui-management)
- [3. Evaluation](#3-evaluation)
  - [3.1 Time Complexity Analysis](#31-time-complexity-analysis)
  - [3.2 Space Complexity Analysis](#32-space-complexity-analysis)
  - [3.3 Data Structure Efficiency](#33-data-structure-efficiency)
  - [3.4 Correctness Guarantees](#34-correctness-guarantees)
- [4. Conclusion](#4-conclusion)

---

## 1. Design

### 1.1 Game Description

**Deadlock Chase** is a 3D *survival-chase* game based on a **discrete grid**, built on top of the Unity Engine. The player controls a cube that moves step-by-step on a grid arena, evading the pursuit of a **Ghost** (AI enemy) that uses an intelligent pathfinding algorithm, while collecting collectibles and power-ups to survive as long as possible and achieve the highest score.

### 1.2 Game Space Design

The game space is represented as a **2D Grid of size W × H** (default 32 × 32 = 1,024 cells) overlaid on a 3D arena. Each grid cell is mapped one-to-one to a world position (`worldPosition`) and has a binary attribute `isWalkable`, determined through a **Physics Sphere-Check** against the `obstacleMask` during initialization.

**Problem Representation — Weighted Graph (Uniform Cost):**

The grid implicitly forms a **uniformly weighted graph** G = (V, E) where:

- **V** = the set of all nodes where `isWalkable == true`.
- **E** = the set of 4-connected adjacency edges (up, down, left, right) with a constant weight **w = 1**.

### 1.3 Entity Elements

| Entity | Description |
|---|---|
| **Player (Cube)** | The player-controlled entity, moving step-by-step on the grid with a 90° rolling mechanism. |
| **Ghost (Enemy AI)** | An autonomous enemy entity that chases the player using Dijkstra's pathfinding. |
| **Collectibles** | Objects spawned stochastically on random walkable nodes; they grant score points. |
| **Power-Ups** | Collectible variants that grant special effects: *Invincible*, *Freeze Ghost*, *Unlimited Stamina*. |
| **Portal (Teleporter)** | An instant teleportation mechanism between two points on the grid. |

### 1.4 Algorithm Paradigm

#### A. Dijkstra's Shortest Path Algorithm

The core algorithm used for Ghost AI pathfinding is **Dijkstra's Algorithm**. This choice is based on the following properties:

- If P* is the shortest path from node `s` to node `t` passing through node `v`, then the sub-paths `s → v` and `v → t` are also shortest paths respectively. This property is satisfied because edge weights are non-negative (w = 1 ≥ 0).
- At each iteration, Dijkstra selects the node with the minimum `gCost` from the *open set*. This greedy decision is **safe** because all weights are ≥ 0, guaranteeing that an extracted node will never be updated again.
- Dijkstra guarantees the **shortest path** on graphs with non-negative weights. Since all edges have weight 1 (uniform), this algorithm is output-equivalent to BFS, but implemented using Dijkstra's structure.

#### B. Greedy Heuristic — Speed Boost AI

The Ghost AI implements a **Random Speed Boost** mechanism — a *stochastic greedy* strategy:

At every random interval `t ∈ [minWait, maxWait]`, the Ghost's speed is doubled for `boostDuration` seconds.

#### C. Stochastic Spawning (Randomized Greedy)

The collectible spawning system uses **Randomized Selection** with the following probability:

- `P(PowerUp) = powerUpChance / 100`, the remainder being normal collectibles (default 30% power-up).
- Spawn positions are chosen **uniformly at random** from walkable nodes, excluding the player's position (a greedy constraint for fairness).

---

## 2. Implementation

### 2.1 Architecture & Project Structure
```
Assets/Scripts/
├── Algorithms/          # Pathfinding algorithms
│   ├── DijkstraPathfinding.cs
│   └── Node.cs
├── Core/                # Core managers & systems
│   ├── GridManager.cs
│   ├── CollectibleManager.cs
│   ├── PowerUpManager.cs
│   └── SceneTransitionManager.cs
├── Entities/            # Game entity logic
│   ├── GhostAI.cs
│   └── PlayerController.cs
├── Environment/         # Environment objects
│   ├── Collectible.cs
│   ├── CollectibleRotator.cs
│   └── UniversalPortal.cs
├── Movement/            # Movement system
│   └── CubeRoller.cs
└── UI/                  # User interface
    ├── CountdownManager.cs
    ├── GameOverUI.cs
    ├── HoldToQuit.cs
    └── SceneTrigger.cs
```

**Design Patterns:**

| Pattern | Location | Purpose |
|---|---|---|
| **Singleton** | `GridManager`, `CollectibleManager`, `PowerUpManager`, `CountdownManager`, `SceneTransitionManager` | Ensures a single global instance and cross-component access. |
| **Component-Based** | All `MonoBehaviour` classes | Separates behavior into independent components following the Unity paradigm. |
| **Observer (via Unity Events)** | `OnTriggerEnter`, Coroutines | Reactive inter-entity interaction through collision callbacks. |
| **Separation of Concerns** | Folders `Algorithms/`, `Core/`, `Entities/`, etc. | Modular separation of responsibilities. |

### 2.2 Core Data Structures

| Data Structure | Location | Purpose |
|---|---|---|
| `Node[,]` (2D Array) | `GridManager.cs` | Stores all grid nodes; O(1) access via index `[x, y]`. |
| `Node` (Class) | `Node.cs` | Stores `gridPosition`, `worldPosition`, `isWalkable`, `gCost`, `parent`. |
| `List<Node>` (Open Set) | `DijkstraPathfinding.cs` | List of unexplored nodes (linear scan for extract-min). |
| `HashSet<Node>` (Closed Set) | `DijkstraPathfinding.cs` | Set of explored nodes; O(1) amortized lookup. |
| `List<Node>` (Path) | `GhostAI.cs` | Stores the pathfinding result from Ghost to Player. |

**`Node` Definition — the smallest unit in the grid graph (`Node.cs`):**
```csharp
public class Node
{
    public Vector2Int gridPosition; // Grid coordinates (x, y)
    public Vector3 worldPosition;   // 3D world position
    public bool isWalkable;         // Traversable or obstacle
    public int gCost;               // Distance from startNode (Dijkstra)
    public Node parent;             // Back-pointer for path retracing

    public void ResetNode()
    {
        gCost = int.MaxValue; // Initialize as ∞ (unexplored)
        parent = null;
    }
}
```

**Grid generation via Physics Scan (`GridManager.GenerateGrid`):**
```csharp
public void GenerateGrid()
{
    nodes = new Node[gridSize.x, gridSize.y];
    Vector3 worldBottomLeft = transform.position
        - Vector3.right * gridSize.x / 2
        - Vector3.forward * gridSize.y / 2;

    for (int x = 0; x < gridSize.x; x++)
        for (int y = 0; y < gridSize.y; y++)
        {
            Vector3 worldPoint = worldBottomLeft
                + Vector3.right * (x + 0.5f)
                + Vector3.forward * (y + 0.5f);
            // Detect obstacles via sphere-cast to the obstacle layer
            bool isObstacle = Physics.CheckSphere(
                worldPoint + Vector3.up * 0.5f, 0.3f, obstacleMask);
            nodes[x, y] = new Node(new Vector2Int(x, y), worldPoint, !isObstacle);
        }
}
```

### 2.3 AI Mechanics — GhostAI

The following is the Ghost AI decision flow for each step:

```
┌───────────────────────────────────────────────────┐
│                  GhostAI Loop                     │
│                                                   │
│  1. Check if Ghost is frozen → delay & retry      │
│  2. Get current position → GetNodeFromWorldPoint  │
│  3. Run Dijkstra(ghost_pos, player_pos)           │
│  4. Get the first node from path → nextNode       │
│  5. Calculate movement direction (diffX / diffY)  │
│  6. Call CubeRoller.Roll(direction, speed)        │
│  7. onComplete → WaitAndMove → return to step 1   │
└───────────────────────────────────────────────────┘
```

**Key characteristics:**

- **Re-Pathfinding per Step:** The Ghost recalculates the Dijkstra path **every single movement step**, ensuring the path is always accurate relative to the player's latest position (*reactive pursuit*).
- **Decision Delay:** A `0.01 second` delay between decisions prevents infinite loops and provides frames for rendering.
- **Speed Boost Coroutine:** The Ghost's speed stochastically fluctuates between `normalSpeed` (1×) and `boostedSpeed` (2×) via `RandomSpeedBoostRoutine()`, with visual feedback through material color changes (red → purple).

**Dijkstra's pathfinding implementation (`DijkstraPathfinding.cs`):**
```csharp
public static List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
{
    Node startNode = GridManager.Instance.GetNodeFromWorldPoint(startPos);
    Node targetNode = GridManager.Instance.GetNodeFromWorldPoint(targetPos);

    List<Node> openSet = new List<Node>();
    HashSet<Node> closedSet = new HashSet<Node>();
    GridManager.Instance.ResetAllNodes();

    startNode.gCost = 0;
    openSet.Add(startNode);

    while (openSet.Count > 0)
    {
        // Extract-Min: linear scan O(V) — main bottleneck
        Node current = openSet[0];
        for (int i = 1; i < openSet.Count; i++)
            if (openSet[i].gCost < current.gCost) current = openSet[i];

        openSet.Remove(current);
        closedSet.Add(current);

        if (current == targetNode) return RetracePath(startNode, targetNode);

        foreach (Node neighbor in GridManager.Instance.GetNeighbors(current))
        {
            if (!neighbor.isWalkable || closedSet.Contains(neighbor)) continue;
            int newCost = current.gCost + 1; // uniform edge weight w=1
            if (newCost < neighbor.gCost)
            {
                neighbor.gCost = newCost;
                neighbor.parent = current;
                if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
            }
        }
    }
    return new List<Node>(); // path not found
}
```

**Ghost step execution per turn (`GhostAI.MakeNextMove`):**
```csharp
void MakeNextMove()
{
    if (PowerUpManager.Instance.IsGhostFrozen)
    { Invoke(nameof(MakeNextMove), 0.5f); return; } // Freeze power-up active

    Node currentNode = GridManager.Instance.GetNodeFromWorldPoint(transform.position);
    currentPath = DijkstraPathfinding.FindPath(transform.position, playerTransform.position);

    if (currentPath != null && currentPath.Count > 0)
    {
        Node nextNode = currentPath[0];
        int diffX = nextNode.gridPosition.x - currentNode.gridPosition.x;
        int diffY = nextNode.gridPosition.y - currentNode.gridPosition.y;

        Vector3 moveDir = diffX != 0
            ? new Vector3(Mathf.Clamp(diffX, -1, 1), 0, 0)
            : new Vector3(0, 0, Mathf.Clamp(diffY, -1, 1));

        roller.Roll(moveDir, ghostSpeedMultiplier, () => StartCoroutine(WaitAndMove()));
    }
}
```

**Stochastic speed boost (`GhostAI.RandomSpeedBoostRoutine`):**
```csharp
private IEnumerator RandomSpeedBoostRoutine()
{
    while (true)
    {
        yield return new WaitForSeconds(Random.Range(minWaitBoost, maxWaitBoost));
        ghostSpeedMultiplier = boostedSpeed;
        ghostRenderer.material.color = boostColor; // visual: red → purple
        yield return new WaitForSeconds(boostDuration);
        ghostSpeedMultiplier = normalSpeed;
        ghostRenderer.material.color = normalColor; // back to red
    }
}
```

### 2.4 Player Mechanics — PlayerController

- **Input Handling:** Supports **WASD** and **Arrow Keys** (4-directional discrete movement).
- **Stamina System:** Sprint implementation with multi-layered mechanics:
  - *Drain* while sprinting: `currentStamina -= drainRate × Δt`
  - *Refill* while idle (with 0.5s delay): `currentStamina += refillRate × Δt`
  - *Cooldown* after sprint ends: `sprintCooldownDuration = 1.5s`
  - *Override* when Unlimited Stamina power-up is active → stamina always full.
- **Grid Validation:** Before moving, the target node's walkability is checked via `GridManager.GetNodeFromWorldPoint()`, ensuring the player cannot pass through obstacles.

**Stamina and sprint cooldown logic (`PlayerController.HandleTimers`):**
```csharp
void HandleTimers()
{
    bool isUnlimited = PowerUpManager.Instance != null
        && PowerUpManager.Instance.IsUnlimitedStamina;

    // Sprint cooldown countdown
    if (!canSprint)
    {
        sprintCooldownTimer -= Time.deltaTime;
        if (sprintCooldownTimer <= 0) canSprint = true;
    }

    if (isSprinting && roller.IsRolling)
    {
        if (!isUnlimited)
        {
            currentStamina -= drainRate * Time.deltaTime;
            if (currentStamina <= 0) { currentStamina = 0; isSprinting = false; }
        }
        else currentStamina = maxStamina; // unlimited stamina power-up
    }
    else if (currentStamina < maxStamina)
    {
        // Refill after 0.5s delay
        if (refillTimer > 0) refillTimer -= Time.deltaTime;
        else currentStamina += refillRate * Time.deltaTime;
    }
    currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
}
```

### 2.5 Movement System — CubeRoller

The cube movement animation uses the **RotateAround** technique, providing a realistic rolling effect:

- 90° rotation around the *pivot point* (bottom edge of the cube) in the direction of movement.
- Rotation speed: `baseRollSpeed × speedMultiplier` degrees/second.
- After rotation completes, the position is **snapped** to the grid via rounding:
  ```csharp
  transform.position = new Vector3(
      Mathf.Round(transform.position.x * 2) / 2f,
      transform.position.y,
      Mathf.Round(transform.position.z * 2) / 2f
  );
  ```
  This prevents the accumulation of floating-point errors over repeated movements.

### 2.6 Power-Up & Collectible System

**Collectible Types:**

| Type | Effect | Duration |
|---|---|---|
| `Normal` | +10 score | — |
| `Invincible` | Player is immune; touching a Ghost = Ghost is destroyed | 5 seconds |
| `Freeze` | Ghost freezes (stops moving) | 5 seconds |
| `UnlimitedStamina` | Stamina does not deplete while sprinting | 5 seconds |

**Spawning Mechanism:**
- Only **one** collectible is active at a time in the arena.
- After being collected, the next collectible is immediately spawned on a random walkable node.
- The `isCollected` flag on `Collectible` prevents *double-collection* from overlapping trigger events.

**Collectible pickup detection (`Collectible.cs`):**
```csharp
public enum CollectibleType { Normal, Invincible, Freeze, UnlimitedStamina }

private void OnTriggerEnter(Collider other)
{
    if (isCollected) return; // guard: prevent double-trigger
    if (other.CompareTag("Player"))
    {
        isCollected = true;
        CollectibleManager.Instance.OnCollected(this); // update score & spawn next
        Destroy(gameObject);
    }
}
```

**Score update and power-up activation (`CollectibleManager.OnCollected`):**
```csharp
public void OnCollected(Collectible item)
{
    currentScore += item.scoreValue;
    scoreText.text = currentScore.ToString();
    FinalScore = currentScore; // stored as static for the GameOver screen

    if (item.type != CollectibleType.Normal)
        PowerUpManager.Instance.ActivatePowerUp(item.type);

    currentSpawnedItem = null;
    SpawnNext(); // immediately spawn the next collectible
}
```

**Power-up activation via Coroutine (`PowerUpManager.cs`):**
```csharp
public void ActivatePowerUp(CollectibleType type)
{
    StopCoroutine(type.ToString()); // reset if still active
    StartCoroutine(type.ToString());
}

IEnumerator Freeze()
{
    IsGhostFrozen = true;  // GhostAI checks this flag every step
    yield return new WaitForSeconds(powerUpDuration);
    IsGhostFrozen = false;
}
// Invincible and UnlimitedStamina follow the same pattern
```

**Visual Polish:**
- Collectibles have a **continuous rotation** animation and an optional **sinusoidal bobbing** effect:
  `y(t) = y₀ + A · sin(2π · f · t)`

### 2.7 Portal & Teleportation System

- Portals use **Trigger-Based Collision** (`OnTriggerEnter`) to detect the player.
- A static `isTeleporting` flag prevents *re-entry* while teleportation is in progress.
- During teleportation: `CubeRoller` is temporarily disabled, velocity is reset, position is moved to `destination`, then `CubeRoller` is re-enabled after a cooldown (1.5s).

**Teleportation sequence (`UniversalPortal.TeleportSequence`):**
```csharp
private IEnumerator TeleportSequence(Transform playerTransform)
{
    isTeleporting = true;

    // 1. Disable movement during teleportation
    CubeRoller roller = playerTransform.GetComponent<CubeRoller>();
    if (roller != null) roller.enabled = false;

    // 2. Reset momentum
    Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
    if (rb != null) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

    // 3. Snap position to the destination node (stay on grid)
    Node targetNode = GridManager.Instance.GetNodeFromWorldPoint(destination.position);
    if (targetNode != null)
        playerTransform.position = targetNode.worldPosition + Vector3.up * 0.5f
            + destination.forward;

    // 4. Re-enable after teleportation
    if (roller != null) roller.enabled = true;
    yield return new WaitForSeconds(cooldown); // 1.5s cooldown to prevent re-entry
    isTeleporting = false;
}
```

### 2.8 Scene & UI Management

**Scene Flow:**

```
MainMenu  ──▶  GameScene  ──▶  GameOver
   ▲                               │
   └───────────────────────────────┘
```

- **3 Scenes:** `MainMenu`, `GameScene`, `GameOver`.
- Scene transitions use `SceneTransitionManager` with a **fade in/out** effect via `CanvasGroup.alpha` lerp.
- `DontDestroyOnLoad` on `SceneTransitionManager` ensures persistence across scenes.

**UI Features:**

| Feature | Implementation |
|---|---|
| **Score Display** | `TextMeshProUGUI` updated in real-time via `CollectibleManager`. |
| **Stamina Bar** | `Slider` with dynamic color changes: cyan (normal), gray (cooldown), yellow (unlimited). |
| **Countdown** | Scale-down animation 1.5× → 1× on countdown text; blocks input until complete. |
| **Final Score** | Score stored as a `static` property `FinalScore`, persistent across scenes. |
| **Hold-to-Quit** | Holding the Escape key for 2 seconds with visual feedback via `fillAmount`. |

---

## 3. Evaluation

### 3.1 Time Complexity Analysis

#### Dijkstra's Pathfinding — `FindPath()`

Let **V** = number of walkable nodes and **E** = number of edges.

**Current implementation (List — linear scan extract-min):**

```
T(V, E) = O(V² + E)
```

- Each iteration: extract-min O(V) performed O(V) times → O(V²).
- Edge relaxation: total O(E) with additional `Contains` check O(V) per neighbor.
- **Worst case** for a 32 × 32 grid: V = 1,024, E ≤ 4V = 4,096 → **T ≈ O(1,048,576)** operations per call.

**If using a Binary Min-Heap (optimization):**

```
T(V, E) = O((V + E) log V)
```

- For the same grid: T ≈ O(5,120 × 10) = **O(51,200)** — **~20× faster**.

#### Other Operations

| Operation | Complexity | Notes |
|---|---|---|
| `GenerateGrid()` | O(W × H) | Called once during `Awake()`. For 32 × 32: O(1,024). |
| `ResetAllNodes()` | O(W × H) | Called every time `FindPath()` runs. |
| `GetNodeFromWorldPoint()` | O(1) | Converts world coordinates to index via arithmetic formula. |
| `GetRandomWalkableNode()` | O(1) avg, O(100) worst | Random sampling with a maximum of 100 attempts. |
| `PlayerController.Update()` | O(1) | Input check, timer decrement, UI update — all constant. |
| **Ghost AI per step** | O(V² + E) | Dijkstra + O(1) move execution. Runs ~1× per step (not per frame). |

### 3.2 Space Complexity Analysis

| Component | Space | Notes |
|---|---|---|
| Grid `Node[,]` | O(W × H) | 32 × 32 = 1,024 nodes |
| Dijkstra Open Set | O(V) worst case | Maximum of all walkable nodes |
| Dijkstra Closed Set | O(V) worst case | Maximum of all walkable nodes |
| Path Result | O(V) worst case | The longest path may traverse all nodes |
| Node fields (`gCost`, `parent`) | O(V) | Stored directly in the node (in-place) |
| **Total** | **O(W × H)** | Dominated by the grid |

For the default grid: **~1,024 nodes × ~40 bytes/node ≈ 40 KB** — extremely lightweight for a real-time application.

### 3.3 Data Structure Efficiency

| Structure | Operation | Complexity | Notes |
|---|---|---|---|
| `Node[,]` | Index access | O(1) | Direct access via 2D array |
| `Node[,]` | `GetNodeFromWorldPoint` | O(1) | Coordinate → index conversion via arithmetic |
| `List<Node>` (Open Set) | Extract-Min | O(n) | **Linear scan — main bottleneck** |
| `List<Node>` (Open Set) | `Contains` | O(n) | Linear search |
| `List<Node>` (Open Set) | `Remove` | O(n) | Element shifting |
| `HashSet<Node>` (Closed Set) | `Contains` | O(1) amortized | Hash-based lookup |
| `HashSet<Node>` (Closed Set) | `Add` | O(1) amortized | Hash-based insertion |
| `GetNeighbors` | Per-node | O(1) | Maximum of 4 neighbors (constant) |

### 3.4 Correctness Guarantees

#### Dijkstra Correctness

- **Invariant:** Every node that enters the `closedSet` has a `gCost` that represents the shortest distance from `startNode`.
- **Proof Sketch:** Since all edge weights w = 1 ≥ 0, greedy extraction from the open set guarantees that no shorter path exists to an already-closed node. This satisfies **Dijkstra's Invariant**.
- **Termination:** Each iteration moves one node from `openSet` to `closedSet`. Since |V| is finite, the loop terminates within ≤ |V| iterations.
- **Completeness:** If a path exists, Dijkstra will find it. If the `openSet` is empty before `targetNode` is reached, the function returns an empty list (handled correctly).

#### Path Retrace Correctness

- `RetracePath()` traverses `parent` pointers from `endNode` to `startNode`, then calls `Reverse()`. This is correct as long as `parent` is set accurately by the relaxation step.

#### Boundary Safety

- `GetNodeFromWorldPoint()` uses `Mathf.Clamp` → indices are always within bounds `[0, gridSize-1]`, preventing `IndexOutOfRangeException`.
- `GetNeighbors()` performs explicit bounds checks before accessing `nodes[checkX, checkY]`.

#### Collision & State Safety

- The `isCollected` flag on `Collectible` prevents *double-collection*.
- The static `isTeleporting` flag on `UniversalPortal` prevents *portal re-entry* while teleportation is in progress.

---

## 4. Conclusion

### Technical Architecture Quality

The architecture of *Deadlock Chase* demonstrates **good** separation of concerns through a modular folder structure (`Algorithms`, `Core`, `Entities`, `Environment`, `Movement`, `UI`) and the use of the Singleton pattern for manager classes. The component-based design follows Unity conventions precisely, enabling reasonable extensibility. The power-up, stamina, and scoring systems are implemented cleanly with clear state management.

### Algorithm Optimality

Dijkstra's algorithm guarantees a **correct shortest path** for solving the pursuit problem on a uniformly weighted graph. In the context of a 32 × 32 grid with real-time performance constraints:

- **Functionally:** The algorithm works **100% correctly** — the shortest path is always found, boundaries are always safe, and edge cases are handled properly.
- **Performance-wise:** The current implementation runs in O(V²) per call due to the use of `List` as the open set. For a 1,024-node grid, this produces ~10⁶ operations per Ghost step — **still within real-time tolerance** (~60 FPS) for a single agent.
- **Optimization potential:** Substituting `List` with `PriorityQueue` / Min-Heap would reduce the complexity to O((V+E) log V), providing **~20×** headroom for multi-agent scenarios or larger grids.

### Scalability

Storing pathfinding data *in-place* on grid nodes (`gCost`, `parent`) limits the use of concurrent multi-agent pathfinding. The solution is to use **per-query state** (local dictionary) instead of global node mutation, allowing multiple Ghosts to operate independently without race conditions.

### Verdict

> **Deadlock Chase** has a **solid and functional** architecture for the current game scale, with a pathfinding algorithm that is **correct and output-optimal**. Optimizing the open set data structure and decoupling pathfinding state from grid nodes would make this system **production-grade** for more complex scenarios.

---

