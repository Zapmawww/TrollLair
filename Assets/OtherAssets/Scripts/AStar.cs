using System.Collections.Generic;
using UnityEngine;

public class AStar
{
    public class Node
    {
        public Vector2Int position;
        public Node parent;
        public int gCost;
        public int hCost;
        public int fCost { get { return gCost + hCost; } }

        public Node(Vector2Int position, Node parent, int gCost, int hCost)
        {
            this.position = position;
            this.parent = parent;
            this.gCost = gCost;
            this.hCost = hCost;
        }
    }
    static private int CalculateHeuristic(Vector2Int a, Vector2Int b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        return xDistance + yDistance;
    }
    static private List<Node> GetNeighbors(Node node, int[,] map)
    {
        List<Node> neighbors = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int newX = node.position.x + x;
                int newY = node.position.y + y;

                if (newX >= 0 && newX < map.GetLength(0) && newY >= 0 && newY < map.GetLength(1))
                {
                    if (map[newX, newY] == 0)
                    {
                        // moving cost (sqrt2:1)
                        int gCost = node.gCost + (x != 0 && y != 0 ? 14 : 10);
                        int hCost = CalculateHeuristic(new Vector2Int(newX, newY), node.position);
                        neighbors.Add(new Node(new Vector2Int(newX, newY), node, gCost, hCost));
                    }
                }
            }
        }

        return neighbors;
    }
    static public List<Vector2Int> FindPath(int[,] map, Vector2Int startPoint, Vector2Int endPoint)
    {
        Node startNode = new Node(startPoint, null, 0, CalculateHeuristic(startPoint, endPoint));
        List<Node> openList = new List<Node> { startNode };
        List<Node> closedList = new List<Node>();

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];

            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost || (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode.position == endPoint)
            {
                List<Vector2Int> path = new List<Vector2Int>();
                Node pathNode = currentNode;
                while (pathNode != null)
                {
                    path.Add(pathNode.position);
                    pathNode = pathNode.parent;
                }
                path.Reverse();
                return path;
            }

            List<Node> neighbors = GetNeighbors(currentNode, map);
            foreach (Node neighbor in neighbors)
            {
                if (closedList.Exists(n => n.position == neighbor.position)) continue;

                if (!openList.Exists(n => n.position == neighbor.position))
                {
                    openList.Add(neighbor);
                }
                else
                {
                    Node existingNeighbor = openList.Find(n => n.position == neighbor.position);
                    if (neighbor.gCost < existingNeighbor.gCost)
                    {
                        existingNeighbor.parent = currentNode;
                        existingNeighbor.gCost = neighbor.gCost;
                        existingNeighbor.hCost = CalculateHeuristic(neighbor.position, endPoint);
                    }
                }
            }
        }
        return null; // No path found
    }
}