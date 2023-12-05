using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PathFinder
{

    public static string PathListToString (List<Vector2Int> path)
    {
        string s = "";

        foreach (var p in path)
        {
            s += "(" + p.x + ", " + p.y + ") ";
        }

        return s;
    }




    public static List<Vector2Int> CalculateShortestPath(int[,] grid, Vector2Int start, Vector2Int destination)
    {
        // Check if the start and destination positions are valid within the grid
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        if (start.x < 0 || start.x >= rows || start.y < 0 || start.y >= cols ||
            destination.x < 0 || destination.x >= rows || destination.y < 0 || destination.y >= cols)
        {
            Debug.LogError("Invalid start or destination position!");
            return null;
        }

        // Define the possible directions (up, right, down, left)
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        // Initialize the visited array to keep track of visited positions
        bool[,] visited = new bool[rows, cols];
        visited[start.x, start.y] = true;

        // Create a queue for BFS traversal
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(start);

        // Create a dictionary to store the parent positions for each visited position
        Dictionary<Vector2Int, Vector2Int> parentMap = new Dictionary<Vector2Int, Vector2Int>();

        // Perform BFS until the destination position is reached
        while (queue.Count > 0)
        {
            Vector2Int currentPosition = queue.Dequeue();

            // Check if the destination position is reached
            if (currentPosition == destination)
            {
                break;
            }

            // Explore the neighboring positions
            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighborPosition = currentPosition + direction;

                // Check if the neighboring position is within the grid boundaries and not visited
                if (neighborPosition.x >= 0 && neighborPosition.x < rows &&
                    neighborPosition.y >= 0 && neighborPosition.y < cols &&
                    !visited[neighborPosition.x, neighborPosition.y] &&
                    grid[neighborPosition.x, neighborPosition.y] == 0)
                {
                    visited[neighborPosition.x, neighborPosition.y] = true;
                    queue.Enqueue(neighborPosition);
                    parentMap[neighborPosition] = currentPosition;
                }
            }
        }

        // Reconstruct the shortest path using the parentMap
        List<Vector2Int> shortestPath = new List<Vector2Int>();
        Vector2Int currentPos = destination;

        while (currentPos != start)
        {
            shortestPath.Add(currentPos);
            if (!parentMap.ContainsKey(currentPos))
            {
                //Debug.LogError("No path found between the start and destination positions!");
                return null;
            }
            currentPos = parentMap[currentPos];
        }

        // Reverse the path to get it from start to destination
        shortestPath.Reverse();

        return shortestPath;
    }
}
