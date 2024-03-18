using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviourEngine : MonoBehaviour
{
    public Vector2Int GenerateEnemyTurn(int[,] grid)
    {
        // Get grid dimensions
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        // TODO: Estúpido. Mudar quando implementação de AI for feita
        int x, y, distance;
        do
        {
            x = Random.Range(0, rows);
            y = Random.Range(0, cols);

            var pathToThisPlace = PathFinder.CalculateShortestPath(grid, gameObject.GetComponent<UnitMovement>().GetGridCoordinates(), new(x, y));

            distance = (pathToThisPlace != null) ? pathToThisPlace.Count : 99;
        } while (grid[x, y] == 1 || grid[x, y] == 2 || grid[x, y] == 3 || distance > gameObject.GetComponent<UnitInfo>().MovementRange);

        return new Vector2Int(x, y);
    }
}
