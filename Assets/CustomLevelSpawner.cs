using PopperBurst;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PopperTypeConverter
{
    public static PopperColor ToPopperColor(PopperType type)
    {
        return type switch
        {
            PopperType.Purple => PopperColor.Purple,
            PopperType.Blue => PopperColor.Blue,
            PopperType.Yellow => PopperColor.Yellow,
            PopperType.Empty => (PopperColor)0, // Empty
            _ => PopperColor.Purple // Default fallback
        };
    }

    public static PopperType FromPopperColor(PopperColor color)
    {
        return color switch
        {
            PopperColor.Purple => PopperType.Purple,
            PopperColor.Blue => PopperType.Blue,
            PopperColor.Yellow => PopperType.Yellow,
            _ => PopperType.Empty
        };
    }

    public static Color GetVisualColor(PopperType type)
    {
        return type switch
        {
            PopperType.Purple => new Color(0.5f, 0f, 0.8f), // Purple
            PopperType.Blue => Color.blue,
            PopperType.Yellow => Color.yellow,
            _ => Color.gray // Empty
        };
    }
}

// CUSTOM LEVEL SPAWNER
public class CustomLevelSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private GameObject popperPrefab;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private float cellSize = 1.5f;
    [SerializeField] private Vector2 gridOffset = Vector2.zero;

    [Header("Animation Settings")]
    [SerializeField] private bool enableFallingAnimation = true;
    [SerializeField] private float fallHeight = 10f;
    [SerializeField] private float fallDuration = 0.8f;
    [SerializeField] private AnimationCurve fallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float columnDelay = 0.1f;
    [SerializeField] private float rowDelay = 0.05f;

    [Header("Visual Settings")]
    [SerializeField] private bool showGridGizmos = true;
    [SerializeField] private Color gridGizmoColor = Color.yellow;

    // Runtime data
    private PopperController[,] spawnedPoppers;
    private Transform gridParent;
    private Vector2 gridStartPosition;

    // Animation tracking
    private List<Coroutine> activeAnimations = new List<Coroutine>();
    private bool isSpawning = false;

    // Properties
    private int GridWidth = 0;
    private LevelData _currentLevelData;
    public int GridHeight = 0;

    public int _totalPoppers => gridParent.childCount;
    void Awake()
    {
        if (!gameCamera)
            gameCamera = Camera.main;

        CreateGridParent();
    }

    public void BuildLevel(LevelData levelData)
    {
        _currentLevelData = levelData;

        if (levelData)
        {
            SpawnLevel(levelData);
        }
        else
        {
            Debug.LogWarning("No level data assigned! Please assign a LevelData ScriptableObject.");
        }
    }

    private void CreateGridParent()
    {
        GameObject gridObj = new GameObject("Grid Parent");
        gridObj.transform.SetParent(transform);
        gridParent = gridObj.transform;
    }

    public void SpawnLevel(LevelData levelData)
    {
        GridHeight = levelData.rows;
        GridWidth = levelData.cols;

        if (!ValidateLevelData())
        {
            Debug.LogError("Invalid level data! Please check your level configuration.");
            return;
        }

        if (isSpawning)
        {
            Debug.LogWarning("Already spawning a level! Please wait...");
            return;
        }

        ClearExistingPoppers();
        CalculateGridPosition();

        if (enableFallingAnimation)
        {
            StartCoroutine(SpawnPoppersWithAnimation());
        }
        else
        {
            SpawnPoppersInstantly();
        }

        CenterCameraOnGrid();

        Debug.Log($"Spawning level: {GridWidth}x{GridHeight} score {levelData.targetScore}taps{levelData.maxTaps}");
    }

    private bool ValidateLevelData()
    {
        if (!_currentLevelData)
        {
            Debug.LogError("No level data assigned!");
            return false;
        }

        if (_currentLevelData.rows <= 0 || _currentLevelData.cols <= 0)
        {
            Debug.LogError("Invalid grid dimensions!");
            return false;
        }

        int expectedLength = _currentLevelData.rows * _currentLevelData.cols;
        if (_currentLevelData.gridData == null || _currentLevelData.gridData.Length != expectedLength)
        {
            Debug.LogError($"Grid data length mismatch! Expected: {expectedLength}, Got: {(_currentLevelData.gridData?.Length ?? 0)}");
            return false;
        }

        return true;
    }

    private void ClearExistingPoppers()
    {
        // Stop any running animations
        StopAllAnimations();

        if (gridParent)
        {
            // Destroy existing poppers
            for (int i = gridParent.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                    Destroy(gridParent.GetChild(i).gameObject);
                else
                    DestroyImmediate(gridParent.GetChild(i).gameObject);
            }
        }

        spawnedPoppers = new PopperController[GridWidth, GridHeight];
    }

    private void StopAllAnimations()
    {
        isSpawning = false;

        foreach (Coroutine animation in activeAnimations)
        {
            if (animation != null)
                StopCoroutine(animation);
        }
        activeAnimations.Clear();
    }

    private void CalculateGridPosition()
    {
        // Calculate the total grid size
        Vector2 totalGridSize = new Vector2(
            (GridWidth - 1) * cellSize,
            (GridHeight - 1) * cellSize
        );

        // Center the grid on screen
        gridStartPosition = -totalGridSize * 0.5f + gridOffset;
    }

    private IEnumerator SpawnPoppersWithAnimation()
    {
        isSpawning = true;

        // Create a list to track poppers by column for cascading effect
        Dictionary<int, List<PopperSpawnData>> columnPoppers = new Dictionary<int, List<PopperSpawnData>>();

        // First pass: Create all poppers and organize by column
        for (int row = 0; row < GridHeight; row++)
        {
            for (int col = 0; col < GridWidth; col++)
            {
                int index = row * GridWidth + col;
                PopperType popperType = _currentLevelData.gridData[index];

                // Skip empty cells
                if (popperType == PopperType.Empty) continue;

                // Convert editor coordinates to Unity world coordinates
                int unityRow = (GridHeight - 1) - row;
                Vector3 targetPosition = GridToWorldPosition(col, unityRow);
                Vector3 startPosition = targetPosition + Vector3.up * fallHeight;

                // Create popper at start position (above screen)
                GameObject popperObj = Instantiate(popperPrefab, startPosition, Quaternion.identity, gridParent);
                PopperController popper = SetupPopper(popperObj, popperType, col, unityRow, row);

                // Organize by column
                if (!columnPoppers.ContainsKey(col))
                    columnPoppers[col] = new List<PopperSpawnData>();

                columnPoppers[col].Add(new PopperSpawnData
                {
                    popperObj = popperObj,
                    popper = popper,
                    startPosition = startPosition,
                    targetPosition = targetPosition,
                    row = unityRow,
                    editorRow = row
                });
            }
        }

        // Second pass: Animate columns with delays
        for (int col = 0; col < GridWidth; col++)
        {
            if (columnPoppers.ContainsKey(col))
            {
                // Start column animation
                Coroutine columnAnimation = StartCoroutine(AnimateColumn(columnPoppers[col]));
                activeAnimations.Add(columnAnimation);
            }

            // Wait before starting next column
            yield return new WaitForSeconds(columnDelay);
        }

        // Wait for all animations to complete
        yield return new WaitForSeconds(fallDuration + (GridHeight * rowDelay));

        isSpawning = false;
        Debug.Log($" Spawned level with falling animation: {GridWidth}x{GridHeight} with {gridParent.childCount} poppers");
    }

    private IEnumerator AnimateColumn(List<PopperSpawnData> columnPoppers)
    {
        // Sort by row (top to bottom) for proper falling order
        columnPoppers.Sort((a, b) => b.row.CompareTo(a.row));

        for (int i = 0; i < columnPoppers.Count; i++)
        {
            PopperSpawnData data = columnPoppers[i];

            // Start falling animation for this popper
            Coroutine fallAnimation = StartCoroutine(AnimatePopperFall(data));
            activeAnimations.Add(fallAnimation);

            // Small delay between poppers in same column
            yield return new WaitForSeconds(rowDelay);
        }
    }

    private IEnumerator AnimatePopperFall(PopperSpawnData data)
    {
        float elapsed = 0f;

        // Disable popper animations during falling
        if (data.popper != null)
        {
            PopperAnimator animator = data.popper.GetComponent<PopperAnimator>();
            if (animator != null)
                animator.StopAllAnimations();
        }

        while (elapsed < fallDuration && isSpawning)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;

            // Apply animation curve for smooth falling
            float curveValue = fallCurve.Evaluate(t);

            // Interpolate position
            if (data.popperObj != null)
            {
                data.popperObj.transform.position = Vector3.Lerp(
                    data.startPosition,
                    data.targetPosition,
                    curveValue
                );
            }

            yield return null;
        }

        // Ensure final position is exact
        if (data.popperObj != null)
        {
            data.popperObj.transform.position = data.targetPosition;

            // Re-enable popper animations
            if (data.popper != null)
            {
                PopperAnimator animator = data.popper.GetComponent<PopperAnimator>();
                if (animator != null)
                    animator.PlayIdleAnimation();
            }
        }
    }

    private void SpawnPoppersInstantly()
    {
        for (int row = 0; row < GridHeight; row++)
        {
            for (int col = 0; col < GridWidth; col++)
            {
                int index = row * GridWidth + col;
                PopperType popperType = _currentLevelData.gridData[index];

                // Skip empty cells
                if (popperType == PopperType.Empty) continue;

                // Convert editor coordinates to Unity world coordinates
                int unityRow = (GridHeight - 1) - row;
                Vector3 worldPosition = GridToWorldPosition(col, unityRow);

                // Spawn popper at final position
                GameObject popperObj = Instantiate(popperPrefab, worldPosition, Quaternion.identity, gridParent);
                SetupPopper(popperObj, popperType, col, unityRow, row);
            }
        }

        Debug.Log($" Spawned level instantly: {GridWidth}x{GridHeight} with {gridParent.childCount} poppers");
    }

    private PopperController SetupPopper(GameObject popperObj, PopperType popperType, int col, int unityRow, int editorRow)
    {
        PopperController popper = popperObj.GetComponent<PopperController>();
        if (popper)
        {
            // Convert PopperType to PopperColor and set initial state
            PopperColor targetColor = PopperTypeConverter.ToPopperColor(popperType);
            popper.ForceAdvanceToColor(targetColor);
            spawnedPoppers[col, unityRow] = popper;

            // Add grid position component for easy reference
            GridPositionComponent gridPos = popperObj.GetComponent<GridPositionComponent>();
            if (!gridPos)
                gridPos = popperObj.AddComponent<GridPositionComponent>();
            gridPos.gridPosition = new GridPosition(col, unityRow);
        }

        // Name for easier debugging (show editor coordinates for clarity)
        popperObj.name = $"Popper Editor({col}, {editorRow}) Unity({col}, {unityRow}) - {popperType}";

        return popper;
    }

    // Helper class for animation data
    private class PopperSpawnData
    {
        public GameObject popperObj;
        public PopperController popper;
        public Vector3 startPosition;
        public Vector3 targetPosition;
        public int row;
        public int editorRow;
    }

    private void CenterCameraOnGrid()
    {
        if (!gameCamera) return;

        // Calculate grid center in world space
        Vector3 gridCenter = GridToWorldPosition(GridWidth / 2f, GridHeight / 2f);

        // Position camera to center on grid
        Vector3 cameraPos = gameCamera.transform.position;
        cameraPos.x = gridCenter.x;
        cameraPos.y = gridCenter.y;
        gameCamera.transform.position = cameraPos;

        // Adjust camera size to fit grid (for orthographic cameras)
        if (gameCamera.orthographic)
        {
            float gridSizeX = GridWidth * cellSize;
            float gridSizeY = GridHeight * cellSize;
            float aspectRatio = gameCamera.aspect;

            float requiredSize = Mathf.Max(gridSizeY * 0.6f, gridSizeX * 0.6f / aspectRatio);
            gameCamera.orthographicSize = requiredSize;
        }
    }

    // UTILITY METHODS
    public Vector3 GridToWorldPosition(float gridX, float gridY)
    {
        return new Vector3(
            gridStartPosition.x + gridX * cellSize,
            gridStartPosition.y + gridY * cellSize,
            0
        );
    }

    public Vector2 WorldToGridPosition(Vector3 worldPos)
    {
        Vector2 localPos = worldPos - (Vector3)gridStartPosition;
        return new Vector2(
            Mathf.RoundToInt(localPos.x / cellSize),
            Mathf.RoundToInt(localPos.y / cellSize)
        );
    }

    public PopperController GetPopperAt(int col, int row)
    {
        if (IsValidGridPosition(col, row))
            return spawnedPoppers[col, row];
        return null;
    }

    // Get popper using editor coordinates (for easier debugging)
    public PopperController GetPopperAtEditorCoords(int editorCol, int editorRow)
    {
        int unityRow = (GridHeight - 1) - editorRow;
        return GetPopperAt(editorCol, unityRow);
    }

    public bool IsValidGridPosition(int col, int row)
    {
        return col >= 0 && col < GridWidth && row >= 0 && row < GridHeight;
    }

    public List<PopperController> GetNeighbors(int col, int row)
    {
        List<PopperController> neighbors = new List<PopperController>();
        int[] dx = { 0, 0, -1, 1 }; // Up, Down, Left, Right
        int[] dy = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int newCol = col + dx[i];
            int newRow = row + dy[i];

            PopperController neighbor = GetPopperAt(newCol, newRow);
            if (neighbor != null)
                neighbors.Add(neighbor);
        }

        return neighbors;
    }

    public void RemovePopperFromGrid(int col, int row)
    {
        if (IsValidGridPosition(col, row))
        {
            spawnedPoppers[col, row] = null;
        }
    }

    // LEVEL MANAGEMENT METHODS
    public void LoadLevel(LevelData levelData)
    {
        SpawnLevel(levelData);
    }

    public void ReloadCurrentLevel()
    {
        if (_currentLevelData)
            SpawnLevel(_currentLevelData);
    }

    // DEBUG AND TESTING METHODS
    [ContextMenu("Reload Current Level")]
    public void ReloadCurrentLevelFromContext()
    {
        ReloadCurrentLevel();
    }

    [ContextMenu("Spawn With Animation")]
    public void SpawnWithAnimationFromContext()
    {
        if (_currentLevelData)
        {
            enableFallingAnimation = true;
            SpawnLevel(_currentLevelData);
        }
    }

    [ContextMenu("Spawn Instantly")]
    public void SpawnInstantlyFromContext()
    {
        if (_currentLevelData)
        {
            enableFallingAnimation = false;
            SpawnLevel(_currentLevelData);
        }
    }

    [ContextMenu("Clear Grid")]
    public void ClearGrid()
    {
        ClearExistingPoppers();
    }

    [ContextMenu("Debug Grid Layout")]
    public void DebugGridLayout()
    {
        if (!_currentLevelData)
        {
            Debug.LogWarning("No level data to debug!");
            return;
        }

        Debug.Log("=== Grid Layout Debug ===");
        Debug.Log($"Grid Size: {GridWidth} x {GridHeight}");

        for (int row = 0; row < GridHeight; row++)
        {
            string rowString = $"Editor Row {row}: ";
            for (int col = 0; col < GridWidth; col++)
            {
                int index = row * GridWidth + col;
                PopperType type = _currentLevelData.gridData[index];
                rowString += $"[{type}] ";
            }
            Debug.Log(rowString);
        }

        Debug.Log("=== Unity World Coordinates ===");
        for (int row = 0; row < GridHeight; row++)
        {
            int unityRow = (GridHeight - 1) - row;
            Vector3 worldPos = GridToWorldPosition(0, unityRow);
            Debug.Log($"Editor Row {row} → Unity Row {unityRow} → World Y: {worldPos.y:F1}");
        }
    }

    // GIZMOS FOR VISUAL DEBUGGING
    void OnDrawGizmos()
    {
        if (!showGridGizmos || !_currentLevelData) return;

        Gizmos.color = gridGizmoColor;

        // Draw grid cells
        for (int col = 0; col < GridWidth; col++)
        {
            for (int row = 0; row < GridHeight; row++)
            {
                Vector3 cellCenter = GridToWorldPosition(col, row);
                Gizmos.DrawWireCube(cellCenter, Vector3.one * cellSize * 0.9f);
            }
        }

        // Draw grid bounds
        Gizmos.color = Color.red;
        Vector3 gridCenter = GridToWorldPosition(GridWidth / 2f, GridHeight / 2f);
        Vector3 gridSize = new Vector3(GridWidth * cellSize, GridHeight * cellSize, 0);
        Gizmos.DrawWireCube(gridCenter, gridSize);
    }
}

// LEVEL MANAGER (Optional - for switching between multiple levels)
public class CustomLevelManager : MonoBehaviour
{
    [Header("Level Management")]
    [SerializeField] private LevelData[] allLevels;
    [SerializeField] private int currentLevelIndex = 0;
    [SerializeField] private CustomLevelSpawner levelSpawner;

    void Start()
    {
        if (!levelSpawner)
            levelSpawner = FindObjectOfType<CustomLevelSpawner>();

        LoadCurrentLevel();
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < allLevels.Length)
        {
            currentLevelIndex = levelIndex;
            LoadCurrentLevel();
        }
    }

    public void NextLevel()
    {
        if (currentLevelIndex < allLevels.Length - 1)
        {
            currentLevelIndex++;
            LoadCurrentLevel();
        }
    }

    public void PreviousLevel()
    {
        if (currentLevelIndex > 0)
        {
            currentLevelIndex--;
            LoadCurrentLevel();
        }
    }

    private void LoadCurrentLevel()
    {
        if (levelSpawner && allLevels.Length > 0 && currentLevelIndex < allLevels.Length)
        {
            levelSpawner.LoadLevel(allLevels[currentLevelIndex]);
            Debug.Log($"Loaded Level {currentLevelIndex + 1}: {allLevels[currentLevelIndex].name}");
        }
    }

    [ContextMenu("Load Next Level")]
    public void LoadNextLevelFromContext() => NextLevel();

    [ContextMenu("Load Previous Level")]
    public void LoadPreviousLevelFromContext() => PreviousLevel();
}

// GRID POSITION COMPONENT (If not already defined)
public class GridPositionComponent : MonoBehaviour
{
    public GridPosition gridPosition;

    public void UpdateGridPosition(GridPosition newPosition)
    {
        gridPosition = newPosition;
        name = $"Popper ({gridPosition.x}, {gridPosition.y})";
    }
}

// GRID POSITION STRUCT (If not already defined)
[System.Serializable]
public struct GridPosition
{
    public int x;
    public int y;

    public GridPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override string ToString() => $"({x}, {y})";
}