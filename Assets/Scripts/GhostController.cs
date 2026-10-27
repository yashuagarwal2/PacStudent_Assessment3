using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    public int ghostNumber = 1;
    public Tweener tweener;
    public LevelGenerator level;
    public GameManager gm;
    public Transform pac;
    public float pacSpeed = 5f;

    Vector2Int lastDir = Vector2Int.zero;
    Vector3 homePos;
    bool leftHouse = false;
    int waypoint = 0;

    Vector2Int[] corners =
    {
        new Vector2Int(1, 1),
        new Vector2Int(26, 1),
        new Vector2Int(26, 27),
        new Vector2Int(1, 27)
    };

    Vector2Int[] dirs =
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

    void Start()
    {
        homePos = transform.position;
    }

    void Update()
    {
        if (gm.gameRunning == false)
        {
            return;
        }

        if (tweener.TweenExists(transform))
        {
            return;
        }

        int index = ghostNumber - 1;
        int state = gm.ghostStates[index];

        if (state == GameManager.DEAD)
        {
            GoHome();
            return;
        }

        Vector2Int cur = GetCell(transform.position);

        if (leftHouse == false && InHouse(cur) == false)
        {
            leftHouse = true;
        }

        Vector2Int dir;

        if (leftHouse == false)
        {
            dir = ExitHouse(cur);
        }
        else if (state == GameManager.SCARED || state == GameManager.RECOVERING)
        {
            dir = ChooseDir(cur, 1);
        }
        else
        {
            dir = ChooseDir(cur, ghostNumber);
        }

        if (dir == Vector2Int.zero)
        {
            return;
        }

        float speed;

        if (state == GameManager.NORMAL)
        {
            speed = pacSpeed * 0.9f;
        }
        else
        {
            speed = pacSpeed * 0.45f;
        }

        Vector2Int next = Move(cur, dir);
        lastDir = dir;
        tweener.AddTween(transform, transform.position, GetWorld(next), 1f / speed);
    }

    Vector2Int GetCell(Vector3 p)
    {
        int col = Mathf.RoundToInt(p.x);
        int row = Mathf.RoundToInt(-p.y);
        return new Vector2Int(col, row);
    }

    Vector3 GetWorld(Vector2Int c)
    {
        return new Vector3(c.x, -c.y, 0);
    }

    Vector2Int Move(Vector2Int c, Vector2Int d)
    {
        return new Vector2Int(c.x + d.x, c.y - d.y);
    }

    bool InHouse(Vector2Int c)
    {
        if (c.y >= 12 && c.y <= 16 && c.x >= 10 && c.x <= 17)
        {
            return true;
        }
        return false;
    }

    bool InTunnel(Vector2Int c)
    {
        if (c.y == 14 && (c.x < 6 || c.x > 21))
        {
            return true;
        }
        return false;
    }

    bool CanGhostWalk(Vector2Int c)
    {
        if (level.IsWalkable(c.y, c.x) == false)
        {
            return false;
        }

        if (InHouse(c) || InTunnel(c))
        {
            return false;
        }

        return true;
    }

    Vector2Int ExitHouse(Vector2Int cur)
    {
        Vector2Int target;

        if (ghostNumber == 1 || ghostNumber == 3)
        {
            target = new Vector2Int(13, 11);
        }
        else
        {
            target = new Vector2Int(13, 17);
        }

        Vector2Int best = Vector2Int.zero;
        float bestDist = 99999f;

        for (int i = 0; i < 4; i++)
        {
            Vector2Int n = Move(cur, dirs[i]);

            bool inside = n.y >= 13 && n.y <= 15 && n.x >= 11 && n.x <= 16;
            bool door = level.fullMap[n.y, n.x] == 8;
            bool isTarget = n == target;

            if (inside == false && door == false && isTarget == false)
            {
                continue;
            }

            float dist = Vector2.Distance(n, target);

            if (dist < bestDist)
            {
                bestDist = dist;
                best = dirs[i];
            }
        }

        return best;
    }

    Vector2Int ChooseDir(Vector2Int cur, int behaviour)
    {
        List<Vector2Int> options = new List<Vector2Int>();

        for (int i = 0; i < 4; i++)
        {
            Vector2Int d = dirs[i];

            if (d == -lastDir)
            {
                continue;
            }

            if (CanGhostWalk(Move(cur, d)))
            {
                options.Add(d);
            }
        }

        if (options.Count == 0)
        {
            Vector2Int back = -lastDir;

            if (lastDir != Vector2Int.zero && CanGhostWalk(Move(cur, back)))
            {
                return back;
            }

            return Vector2Int.zero;
        }

        if (behaviour == 1)
        {
            return PickByDistance(cur, options, true);
        }

        if (behaviour == 2)
        {
            return PickByDistance(cur, options, false);
        }

        if (behaviour == 3)
        {
            return options[Random.Range(0, options.Count)];
        }

        return GoClockwise(cur, options);
    }

    Vector2Int PickByDistance(Vector2Int cur, List<Vector2Int> options, bool runAway)
    {
        float curDist = Vector3.Distance(GetWorld(cur), pac.position);
        List<Vector2Int> good = new List<Vector2Int>();

        for (int i = 0; i < options.Count; i++)
        {
            Vector3 nextPos = GetWorld(Move(cur, options[i]));
            float newDist = Vector3.Distance(nextPos, pac.position);

            if (runAway && newDist >= curDist)
            {
                good.Add(options[i]);
            }

            if (runAway == false && newDist <= curDist)
            {
                good.Add(options[i]);
            }
        }

        if (good.Count == 0)
        {
            good = options;
        }

        return good[Random.Range(0, good.Count)];
    }

    Vector2Int GoClockwise(Vector2Int cur, List<Vector2Int> options)
    {
        if (cur == corners[waypoint])
        {
            waypoint = waypoint + 1;

            if (waypoint == 4)
            {
                waypoint = 0;
            }
        }

        Vector2Int best = options[0];
        float bestDist = 99999f;

        for (int i = 0; i < options.Count; i++)
        {
            float dist = Vector2.Distance(Move(cur, options[i]), corners[waypoint]);

            if (dist < bestDist)
            {
                bestDist = dist;
                best = options[i];
            }
        }

        return best;
    }

    void GoHome()
    {
        float distance = Vector3.Distance(transform.position, homePos);

        if (distance < 0.01f)
        {
            transform.position = homePos;
            lastDir = Vector2Int.zero;
            leftHouse = false;
            gm.GhostReachedHome(ghostNumber - 1);
            return;
        }

        float time = distance / (pacSpeed * 0.45f);
        tweener.AddTween(transform, transform.position, homePos, time);
    }

    public void ResetGhost()
    {
        tweener.RemoveTween(transform);
        transform.position = homePos;
        lastDir = Vector2Int.zero;
        leftHouse = false;
        waypoint = 0;
    }
}