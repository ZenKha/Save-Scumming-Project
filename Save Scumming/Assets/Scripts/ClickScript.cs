using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickScript : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private int _gridX;
    [SerializeField] private int _gridY;

    public int GridX => _gridX;

    public int GridY => _gridY;


    public void SetGrid (int x, int y)
    {
        _gridX = x;
        _gridY = y;
    }

    private void OnMouseUp()
    {
        GetComponentInParent<GridManager>().MapClick(_gridX, _gridY);
    }
}
