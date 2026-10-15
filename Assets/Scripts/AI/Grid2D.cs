using System.Collections.Generic;
using UnityEngine;

public class Grid2D : MonoBehaviour
{
    [Header("Налаштування сітки")]
    public LayerMask obstacleMask; 
    public Vector2 gridWorldSize = new Vector2(20, 20); 
    public float nodeRadius = 0.25f; 

    [Header("Налаштування Heatmap")]
    public Transform playerTransform;
    public float heatAddRate = 2.0f;
    public float maxHeatValue = 10.0f;
    public float heatDecayRate = 0.3f;
    public bool visualizeHeatmap = true;

    private PathNode[,] grid;
    private float nodeDiameter;
    private int gridSizeX, gridSizeY;

    private void Awake()
    {
        nodeDiameter = nodeRadius * 2f;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    private void Update()
    {
        UpdateHeatmap();
    }

    public void CreateGrid()
    {
        grid = new PathNode[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft 
                                     + Vector3.right * (x * nodeDiameter + nodeRadius) 
                                     + Vector3.up * (y * nodeDiameter + nodeRadius);

                bool walkable = !(Physics2D.OverlapCircle(worldPoint, nodeRadius, obstacleMask));
                grid[x, y] = new PathNode(walkable, worldPoint, x, y);
            }
        }
    }

    private void UpdateHeatmap()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (grid[x, y].heatValue > 0)
                {
                    grid[x, y].CoolDown(heatDecayRate * Time.deltaTime);
                }
            }
        }

        if (playerTransform != null)
        {
            PathNode playerNode = NodeFromWorldPoint(playerTransform.position);
            if (playerNode != null && playerNode.isWalkable)
            {
                playerNode.AddHeat(heatAddRate * Time.deltaTime, maxHeatValue);
            }
        }
    }

    public PathNode NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y];
    }

    public List<PathNode> GetNeighbors(PathNode node)
    {
        List<PathNode> neighbors = new List<PathNode>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbors;
    }

    public PathNode GetHottestNodeInRadius(Vector3 centerPosition, float radius)
    {
        PathNode centerNode = NodeFromWorldPoint(centerPosition);
        int searchRange = Mathf.CeilToInt(radius / nodeDiameter);

        PathNode hottestNode = null;
        float maxFoundHeat = -1f;

        for (int x = -searchRange; x <= searchRange; x++)
        {
            for (int y = -searchRange; y <= searchRange; y++)
            {
                int checkX = centerNode.gridX + x;
                int checkY = centerNode.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    PathNode node = grid[checkX, checkY];
                    if (node.isWalkable && node.heatValue > maxFoundHeat)
                    {
                        maxFoundHeat = node.heatValue;
                        hottestNode = node;
                    }
                }
            }
        }
        return hottestNode;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));

        if (grid != null)
        {
            foreach (PathNode n in grid)
            {
                if (!n.isWalkable)
                {
                    Gizmos.color = new Color(1, 0, 0, 0.3f);
                }
                else if (visualizeHeatmap && n.heatValue > 0.05f)
                {
                    float heatNormalized = n.heatValue / maxHeatValue;
                    Gizmos.color = Color.Lerp(Color.yellow, Color.red, heatNormalized);
                }
                else
                {
                    Gizmos.color = new Color(1, 1, 1, 0.05f);
                }
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.05f));
            }
        }
    }
}