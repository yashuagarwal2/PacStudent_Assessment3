using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public GameObject outsideCorner;
    public GameObject outsideWall;
    public GameObject insideCorner;
    public GameObject insideWall;
    public GameObject tJunction;
    public GameObject ghostDoor;
    public GameObject pellet;
    public GameObject powerPellet;
    public GameObject manualLevel;

    
    int[,] levelMap =
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

    int[,] fullMap;
    bool[,] isVertical;
    int fullRows;
    int fullCols;

    void Start()
    {
        Destroy(manualLevel);

        MakeFullMap();
        FindWallDirections();
        BuildLevel();
        FitCamera();
    }

  
    void MakeFullMap()
    {
        int rows = levelMap.GetLength(0);
        int cols = levelMap.GetLength(1);

        fullRows = rows * 2 - 1;
        fullCols = cols * 2;
        fullMap = new int[fullRows, fullCols];

        for (int r = 0; r < fullRows; r++)
        {
            for (int c = 0; c < fullCols; c++)
            {
                int row = r;
                int col = c;

                if (r >= rows) row = 2 * (rows - 1) - r;
                if (c >= cols) col = 2 * cols - 1 - c;

                fullMap[r, c] = levelMap[row, col];
            }
        }
    }

    
    void FindWallDirections()
    {
        isVertical = new bool[fullRows, fullCols];

        for (int r = 0; r < fullRows; r++)
        {
            for (int c = 0; c < fullCols; c++)
            {
                if (!IsStraight(fullMap[r, c])) continue;

                int sideWalls = 0;
                if (IsWall(r, c - 1)) sideWalls++;
                if (IsWall(r, c + 1)) sideWalls++;

                int upDownWalls = 0;
                if (IsWall(r - 1, c)) upDownWalls++;
                if (IsWall(r + 1, c)) upDownWalls++;

                isVertical[r, c] = upDownWalls > sideWalls;
            }
        }
    }

    void BuildLevel()
    {
        Transform parent = new GameObject("GeneratedLevel").transform;

        for (int r = 0; r < fullRows; r++)
        {
            for (int c = 0; c < fullCols; c++)
            {
                int value = fullMap[r, c];
                GameObject prefab = GetPrefab(value);

                if (prefab == null) continue;

                float angle = GetAngle(r, c, value);
                Instantiate(prefab, new Vector3(c, -r, 0), Quaternion.Euler(0, 0, angle), parent);
            }
        }
    }

    GameObject GetPrefab(int value)
    {
        if (value == 1) return outsideCorner;
        if (value == 2) return outsideWall;
        if (value == 3) return insideCorner;
        if (value == 4) return insideWall;
        if (value == 5) return pellet;
        if (value == 6) return powerPellet;
        if (value == 7) return tJunction;
        if (value == 8) return ghostDoor;
        return null;
    }

    float GetAngle(int r, int c, int value)
    {
        if (IsStraight(value))
        {
            if (isVertical[r, c]) return 90f;
            return 0f;
        }
        if (value == 1 || value == 3) return CornerAngle(r, c);
        if (value == 7) return TJunctionAngle(r, c);

        return 0f; // pellets
    }

    bool InMap(int r, int c)
    {
        return r >= 0 && r < fullRows && c >= 0 && c < fullCols;
    }

    bool IsWall(int r, int c)
    {
        if (!InMap(r, c)) return false;

        int v = fullMap[r, c];
        return v == 1 || v == 2 || v == 3 || v == 4 || v == 7 || v == 8;
    }

    bool IsStraight(int v)
    {
        return v == 2 || v == 4 || v == 8;
    }

   
    int Score(int r, int c, bool wantVertical)
    {
        if (!InMap(r, c)) return 0;

        int v = fullMap[r, c];

        if (IsStraight(v))
        {
            if (isVertical[r, c] == wantVertical) return 2;
            return 0;
        }

        if (v == 1 || v == 3 || v == 7) return 1;

        return 0;
    }

    float CornerAngle(int r, int c)
    {
        int left = Score(r, c - 1, false);
        int right = Score(r, c + 1, false);
        int up = Score(r - 1, c, true);
        int down = Score(r + 1, c, true);

        bool useRight = right >= left;
        bool useDown = down >= up;

        if (useRight && useDown) return 0f;    // right and down
        if (useRight && !useDown) return 90f;  // up and right
        if (!useRight && !useDown) return 180f; // left and up
        return 270f;                            // down and left
    }

    
    float TJunctionAngle(int r, int c)
    {
        if (!IsWall(r - 1, c)) return 0f;
        if (!IsWall(r, c - 1)) return 90f;
        if (!IsWall(r + 1, c)) return 180f;
        if (!IsWall(r, c + 1)) return 270f;
        return 0f;
    }

    void FitCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float middleX = (fullCols - 1) / 2f;
        float middleY = -(fullRows - 1) / 2f;
        cam.transform.position = new Vector3(middleX, middleY, -10f);

        float sizeForHeight = fullRows / 2f;
        float sizeForWidth = fullCols / 2f / cam.aspect;
        cam.orthographicSize = Mathf.Max(sizeForHeight, sizeForWidth) + 1f;
    }
}
