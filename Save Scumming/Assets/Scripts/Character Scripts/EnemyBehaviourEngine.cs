using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public enum Actions { Attack, Block, Rest }

public class EnemyBehaviourEngine : MonoBehaviour
{
    public GameObject target;

    public EnemyTurn GenerateEnemyTurn(int[,] grid, List<GameObject> characters)
    {
        // Get unit info
        var moveInfo = gameObject.GetComponent<UnitMovement>();
        var unitInfo = gameObject.GetComponent<UnitInfo>();

        // Create start vector
        Vector2Int start = new(moveInfo.GridX, moveInfo.GridY);

        // Get grid dimensions
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        // Select random character to target if target is null
        if (target == null)
        {
            var idx = Random.Range(0, characters.Count);
            target = characters[idx];
        }

        // Get target info
        var targetInfo = target.GetComponent<UnitInfo>();
        var targetMoveInfo = target.GetComponent<UnitMovement>();


        // Define the possible directions (up, right, down, left)
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        // Initialize the visited array to keep track of visited positions
        bool[,] visited = new bool[rows, cols];
        visited[start.x, start.y] = true;

        // Create a queue for BFS traversal
        Queue<Vector2Int> queue = new();
        queue.Enqueue(start);

        // Create a dictionary to store the parent positions for each visited position
        Dictionary<EnemyTurn, int> scoreMap = new();

        // Perform BFS until the destination position is reached
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            // Calculate distance from target
            var distance = PathFinder.CalculateShortestPath(grid, current, targetMoveInfo.GetGridCoordinates(), true).Count();
            
            int score = -distance;

            // Add rest option
            // Give bonus score to rest action if current hp bellow 50% of max
            if (unitInfo.Hp / unitInfo.MaxHp < 0.5)
            {
                scoreMap[new(current, Actions.Rest)] = score + 2;
            }
            else
            {
                scoreMap[new(current, Actions.Rest)] = score;
            }

            // Add block option
            scoreMap[new(current, Actions.Block)] = score + 1;

            // Loop through attack directions
            // TODO: Maybe add accuracy/obstacle penalty to attack
            foreach (Vector2Int direction in directions) 
            {
                // Substract distance from tile score (closer is better)
                score = -distance;

                var pierce = unitInfo.Pierce;
                bool attacksTarget = false;

                for (int i = 1; i <= unitInfo.AttackRange; i++)
                {
                    if (pierce <= 0)
                    {
                        break;
                    }

                    Vector2Int coords = current + (direction * i);

                    //Check if coordinates are inside limits of grid
                    if (coords.x < 0 || coords.x > rows - 1 || coords.y < 0 || coords.y > cols - 1)
                    {
                        break;
                    }

                    if (grid[coords.x, coords.y] == 1) 
                    {
                        // Attacks towards obstacle, lower pierce
                        pierce--;
                    }
                    else if (grid[coords.x, coords.y] == 2)
                    {
                        // Attack in this direction can hit a player character
                        score += 3;

                        var hit = characters.Find(c => c.GetComponent<UnitMovement>().GridX == coords.x && c.GetComponent<UnitMovement>().GridY == coords.y);

                        if (hit == null)
                        {
                            Debug.LogError("Character found in grid but not on character list!");
                            continue;
                        }

                        // Can attack target, big score bonus
                        if (hit == target)
                        {
                            score += 20;
                            attacksTarget = true;
                        }

                        var hitInfo = hit.GetComponent<UnitInfo>();

                        // Can it character bellow 50% hp, bonus score
                        if (hitInfo.Hp/hitInfo.MaxHp < 0.5)
                        {
                            score += 2;
                        }

                        // Can attack target next turn, bonus score
                        // Doesnt take into account obstacles or blocked paths, but good enough
                        var targetDistance = Mathf.Abs(current.x - targetMoveInfo.GridX) + Mathf.Abs(current.y - targetMoveInfo.GridY);
                        if (targetDistance <= unitInfo.AttackRange + unitInfo.MovementRange)
                        {
                            score += 5;
                        }
                    }
                    else if (grid[coords.x, coords.y] == 3)
                    {
                        // Attacking ally, score penalty
                        score -= 1;
                    }

                    pierce--;
                }

                scoreMap[new(current, Actions.Attack, direction, attacksTarget)] = score;
            }

            // Explore the neighboring positions
            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighbour = current + direction;

                // Check if the neighboring position is within the grid boundaries and not visited
                if (neighbour.x >= 0 && neighbour.x < rows && neighbour.y >= 0 && neighbour.y < cols && !visited[neighbour.x, neighbour.y] &&
                    grid[neighbour.x, neighbour.y] == 0 &&
                    PathFinder.CalculateShortestPath(grid, start, neighbour).Count <= unitInfo.MovementRange)
                {
                    visited[neighbour.x, neighbour.y] = true;
                    queue.Enqueue(neighbour);
                }
            }
        }

        int maxValue = scoreMap.Values.Max();
        var bestMove = scoreMap.FirstOrDefault(x => x.Value == maxValue).Key;

        return bestMove;
    }
}

public class EnemyTurn
{
    public Vector2Int Destination {  get; private set; }
    public Actions Action { get; private set; }
    public Vector2Int Direction { get; private set; }
    public bool AttacksTarget { get; private set; }

    public EnemyTurn(Vector2Int destination, Actions action)
    {
        Destination = destination;
        Action = action;
    }

    public EnemyTurn(Vector2Int destination, Actions action, Vector2Int direction, bool attacksTarget)
    {
        Destination = destination;
        Action = action;
        Direction = direction;
        AttacksTarget = attacksTarget;
    }
}
