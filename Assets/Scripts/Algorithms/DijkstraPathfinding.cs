using System.Collections.Generic;
using UnityEngine;

public class DijkstraPathfinding
{
    public static List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = GridManager.Instance.GetNodeFromWorldPoint(startPos);
        Node targetNode = GridManager.Instance.GetNodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null || !startNode.isWalkable || !targetNode.isWalkable) 
            return new List<Node>();

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        GridManager.Instance.ResetAllNodes(); 

        startNode.gCost = 0;
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].gCost < currentNode.gCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }
            
            foreach (Node neighbor in GridManager.Instance.GetNeighbors(currentNode))
            {
                if (!neighbor.isWalkable || closedSet.Contains(neighbor))
                    continue;

                int newMovementCostToNeighbor = currentNode.gCost + 1;

                if (newMovementCostToNeighbor < neighbor.gCost)
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return new List<Node>();
    }

    private static List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
            {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }
}