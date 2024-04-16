using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    [SerializeField] private int _gridX;
    [SerializeField] private int _gridY;

    public int GridX => _gridX;
    public int GridY => _gridY;

    public Vector2Int GetGridCoordinates()
    {
        return new Vector2Int(_gridX, _gridY);
    }

    public void SetGrid(int x, int y, Vector3 worldPosition)
    {
        _gridX = x;
        _gridY = y;
        transform.position = worldPosition;
    }

    private readonly float PerTileMoveTime = 0.10f;

    public IEnumerator SetGridLerp(int x, int y, Vector3 worldPosition)
    {
        float timeElapsed = 0;
        Vector3 initialPos = transform.position;
        while (timeElapsed < PerTileMoveTime)
        {
            transform.position = Vector3.Lerp(initialPos, worldPosition, timeElapsed / PerTileMoveTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        SetGrid(x, y, worldPosition);
    }
}
