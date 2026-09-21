using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private GameObject outsideCorner;
    [SerializeField] private GameObject outsideWall;
    [SerializeField] private GameObject insideCorner;
    [SerializeField] private GameObject insideWall;
    [SerializeField] private GameObject tJunction;
    [SerializeField] private GameObject ghostDoor;
    [SerializeField] private GameObject pellet;
    [SerializeField] private GameObject powerPellet;
    [SerializeField] private GameObject manualLevel;

    // Step 2: the level map from the brief (top-left quadrant only).
    // 0 = empty, 1 = outside corner, 2 = outside wall, 3 = inside corner,
    // 4 = inside wall, 5 = pellet, 6 = power pellet, 7 = T-junction, 8 = ghost door
    // NOTE: if this differs from the array in your brief, use the brief's version.
    private int[,] levelMap =
    {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0}
    };

    // Step 11: to test Map B, comment out the levelMap above and use this one
    // temporarily. Then swap the real map back.
    // private int[,] levelMap =
    // {
    //     {1,2,2,2,2,2},
    //     {2,5,5,5,5,5},
    //     {2,5,5,5,5,5}
    // };

    private int[,] fullMap;
    private bool[,] isVertical;
    private int fullRows;
    private int fullCols;

    void Start()
    {
        // Step 3: destroy the manual level
        Destroy(manualLevel);

        // Step 4: build fullMap by mirroring levelMap
        int rows = levelMap.GetLength(0);
        int cols = levelMap.GetLength(1);

        fullRows = 2 * rows - 1;
        fullCols = 2 * cols;
        fullMap = new int[fullRows, fullCols];

        for (int r = 0; r < fullRows; r++)
        {
            for (int c = 0; c < fullCols; c++)
            {
                int srcRow = r < rows ? r : 2 * (rows - 1) - r;
                int srcCol = c < cols ? c : 2 * cols - 1 - c;
                fullMap[r, c] = levelMap[srcRow, srcCol];
            }
        }

        // Step 7: work out straight wall orientation.
        // This must finish for the whole map before corners are placed,
        // because corners look at their neighbours' orientation.
        isVertical = new bool[fullRows, fullCols];

        for (int r = 0; r < fullRows; r++)
        {
            for (int c = 0; c < fullCols; c++)
            {
                if (!IsStraight(fullMap[r, c])) continue;

                int horizontal = (IsWall(r, c - 1) ? 1 : 0) + (IsWall(r, c + 1) ? 1 : 0);
                int vertical = (IsWall(r - 1, c) ? 1 : 0) + (IsWall(r + 1, c) ? 1 : 0);
                isVertical[r, c] = vertical > horizontal;
            }
        }

        // Step 5: create a parent and place every piece
        Transform parent = new GameObject("GeneratedLevel").transform;

        for (int r = 0; r < fullRows; r++)
        {
            for (int c = 0; c < fullCols; c++)
            {
                int value = fullMap[r, c];
                if (value == 0) continue;

                GameObject prefab = GetPrefab(value);
                if (prefab == null) continue;

                float angle = GetAngle(r, c, value);
                Instantiate(prefab, new Vector3(c, -r, 0), Quaternion.Euler(0, 0, angle), parent);
            }
        }

        // Step 10: fit the camera
        FitCamera();
    }

    // Step 5: pick the prefab by map value
    private GameObject GetPrefab(int value)
    {
        switch (value)
        {
            case 1: return outsideCorner;
            case 2: return outsideWall;
            case 3: return insideCorner;
            case 4: return insideWall;
            case 5: return pellet;
            case 6: return powerPellet;
            case 7: return tJunction;
            case 8: return ghostDoor;
            default: return null;
        }
    }

    // Rotation (degrees around Z) for each kind of piece
    private float GetAngle(int r, int c, int value)
    {
        switch (value)
        {
            case 2:
            case 4:
            case 8:
                return isVertical[r, c] ? 90f : 0f;   // Step 7
            case 1:
            case 3:
                return CornerAngle(r, c);             // Step 8
            case 7:
                return TJunctionAngle(r, c);          // Step 9
            default:
                return 0f;                            // pellets
        }
    }

    // Step 6: safe neighbour check (false outside the map)
    private bool IsWall(int r, int c)
    {
        if (r < 0 || r >= fullMap.GetLength(0) || c < 0 || c >= fullMap.GetLength(1))
        {
            return false;
        }

        int v = fullMap[r, c];
        return v == 1 || v == 2 || v == 3 || v == 4 || v == 7 || v == 8;
    }

    private bool IsStraight(int v)
    {
        return v == 2 || v == 4 || v == 8;
    }

    // Step 8: how strongly does the neighbour at (r, c) suggest a connection?
    // 2 = definite (a straight wall running the right way)
    // 1 = maybe    (a corner or T-junction)
    // 0 = nothing
    private int SideScore(int r, int c, bool wantVertical)
    {
        if (r < 0 || r >= fullMap.GetLength(0) || c < 0 || c >= fullMap.GetLength(1))
        {
            return 0;
        }

        int v = fullMap[r, c];

        if (IsStraight(v))
        {
            return isVertical[r, c] == wantVertical ? 2 : 0;
        }

        if (v == 1 || v == 3 || v == 7)
        {
            return 1;
        }

        return 0;
    }

    // Step 8: corners
    private float CornerAngle(int r, int c)
    {
        // Horizontal side: a straight wall that is NOT vertical is definite.
        int leftScore = SideScore(r, c - 1, false);
        int rightScore = SideScore(r, c + 1, false);
        bool useRight = rightScore >= leftScore;

        // Vertical side: a straight wall that IS vertical is definite.
        int upScore = SideScore(r - 1, c, true);
        int downScore = SideScore(r + 1, c, true);
        bool useDown = downScore >= upScore;

        if (useRight && useDown) return 0f;      // right + down
        if (useRight && !useDown) return 90f;    // up + right
        if (!useRight && !useDown) return 180f;  // left + up
        return 270f;                             // down + left
    }

    // Step 9: T-junctions. The side with no wall is the missing one.
    private float TJunctionAngle(int r, int c)
    {
        if (!IsWall(r - 1, c)) return 0f;    // up missing
        if (!IsWall(r, c - 1)) return 90f;   // left missing
        if (!IsWall(r + 1, c)) return 180f;  // down missing
        if (!IsWall(r, c + 1)) return 270f;  // right missing
        return 0f;
    }

    // Step 10: camera
    private void FitCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        cam.transform.position = new Vector3((fullCols - 1) / 2f, -(fullRows - 1) / 2f, -10f);
        cam.orthographicSize = Mathf.Max(fullRows / 2f, fullCols / 2f / cam.aspect) + 1f;
    }
}