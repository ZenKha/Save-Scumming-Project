using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

    private readonly float PerTileMoveTime = 0.25f;

    public IEnumerator SetGridLerp(int x, int y, Vector3 worldPosition)
    {
        float timeElapsed = 0;
        var mesh = transform.GetChild(0);

        Vector3 initialPos = transform.position;
        Vector3 initialDir = mesh.transform.forward;
        Vector3 dirOfMove = (worldPosition - initialPos).normalized;


        while (timeElapsed < PerTileMoveTime)
        {
            transform.position = Vector3.Lerp(initialPos, worldPosition, timeElapsed / PerTileMoveTime);
            mesh.transform.forward = Vector3.Lerp(initialDir, dirOfMove, timeElapsed / PerTileMoveTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        SetGrid(x, y, worldPosition);
    }
}
