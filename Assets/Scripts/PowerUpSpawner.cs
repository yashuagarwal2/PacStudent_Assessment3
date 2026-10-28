using System.Collections;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    public GameObject speedPrefab;
    public GameObject freezePrefab;
    public LevelGenerator level;
    public GameManager gm;
    public float interval = 10f;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            if (gm.gameRunning == false)
            {
                continue;
            }

            int rows = level.fullMap.GetLength(0);
            int cols = level.fullMap.GetLength(1);

            for (int tries = 0; tries < 200; tries++)
            {
                int r = Random.Range(0, rows);
                int c = Random.Range(0, cols);

                if (IsGoodSpot(r, c))
                {
                    Spawn(r, c);
                    break;
                }
            }
        }
    }

    bool IsGoodSpot(int r, int c)
    {
        if (level.fullMap[r, c] != 0)
        {
            return false;
        }

        if (r >= 12 && r <= 16 && c >= 10 && c <= 17)
        {
            return false;
        }

        if (r == 14 && (c < 6 || c > 21))
        {
            return false;
        }

        if (level.IsWalkable(r, c) == false)
        {
            return false;
        }

        return true;
    }

    void Spawn(int r, int c)
    {
        GameObject prefab;

        if (Random.Range(0, 2) == 0)
        {
            prefab = speedPrefab;
        }
        else
        {
            prefab = freezePrefab;
        }

        Vector3 position = new Vector3(c, -r, 0);
        Instantiate(prefab, position, Quaternion.identity);
    }
}
