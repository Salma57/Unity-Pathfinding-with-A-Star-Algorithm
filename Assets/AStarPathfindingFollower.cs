using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarPathfindingFollower : MonoBehaviour
{
    public Tilemap moveTilemap;
    public Tilemap obstacleTilemap;
    public Transform player;
    public Transform goal;
    private List<Vector3Int> path;
    private Vector3Int lastGoalPosition;
    private bool isMoving;

    void Start()
    {
        lastGoalPosition = moveTilemap.WorldToCell(goal.position);
        path = FindPath(player.position, goal.position);
        StartCoroutine(MoveAlongPath());
    }

    void Update()
    {
        Vector3Int currentGoalPosition = moveTilemap.WorldToCell(goal.position);

        
        if (currentGoalPosition != lastGoalPosition)
        {
            lastGoalPosition = currentGoalPosition;
            path = FindPath(player.position, goal.position);
            if (!isMoving) 
            {
                StartCoroutine(MoveAlongPath());
            }
        }
    }

    List<Vector3Int> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Vector3Int start = moveTilemap.WorldToCell(startPos);
        Vector3Int target = moveTilemap.WorldToCell(targetPos);

        Dictionary<Vector3Int, float> gCost = new Dictionary<Vector3Int, float>();
        Dictionary<Vector3Int, float> fCost = new Dictionary<Vector3Int, float>();
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        List<Vector3Int> openList = new List<Vector3Int> { start };
        HashSet<Vector3Int> closedList = new HashSet<Vector3Int>();

        gCost[start] = 0;
        fCost[start] = Heuristic(start, target);

        while (openList.Count > 0)
        {
            Vector3Int current = openList[0];
            foreach (var node in openList)
                if (fCost[node] < fCost[current])
                    current = node;

            if (current == target)
                return ReconstructPath(cameFrom, current);

            openList.Remove(current);
            closedList.Add(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (closedList.Contains(neighbor) || obstacleTilemap.HasTile(neighbor))
                    continue;

                float tentativeGCost = gCost[current] + Vector3Int.Distance(current, neighbor);

                if (!openList.Contains(neighbor))
                    openList.Add(neighbor);
                else if (tentativeGCost >= gCost[neighbor])
                    continue;

                cameFrom[neighbor] = current;
                gCost[neighbor] = tentativeGCost;
                fCost[neighbor] = gCost[neighbor] + Heuristic(neighbor, target);
            }
        }

        return new List<Vector3Int>();
    }

    float Heuristic(Vector3Int a, Vector3Int b)
    {
        return Vector3Int.Distance(a, b);
    }

    List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
    {
        var path = new List<Vector3Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }

    System.Collections.IEnumerator MoveAlongPath()
    {
        isMoving = true;

        while (true)
        {
            
            if (path.Count == 0 || Vector3.Distance(player.position, goal.position) < 0.1f)
            {
                
                isMoving = false;
                yield break;
            }

            
            Vector3Int nextCell = path[0];
            Vector3 targetPosition = moveTilemap.CellToWorld(nextCell) + new Vector3(0.5f, 0.5f, 0);

            
            while (Vector3.Distance(player.position, targetPosition) > 0.05f)
            {
                player.position = Vector3.MoveTowards(player.position, targetPosition, Time.deltaTime * 3f);
                yield return null;

                
                if (Vector3.Distance(player.position, goal.position) < 0.1f)
                {
                    
                    yield break;
                }
            }

            
            path.RemoveAt(0);
        }
    }

    List<Vector3Int> GetNeighbors(Vector3Int cell)
    {
        var neighbors = new List<Vector3Int>
        {
            cell + Vector3Int.up,
            cell + Vector3Int.down,
            cell + Vector3Int.left,
            cell + Vector3Int.right
        };

        return neighbors;
    }
}
