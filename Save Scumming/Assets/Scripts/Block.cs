using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] bool isClickable;

    [Header("Coordinates")]
    [SerializeField] private int _gridX;
    [SerializeField] private int _gridY;

    public int GridX => _gridX;
    public int GridY => _gridY;

    [Space(10)] [Header("Highlight Tiles")]
    [SerializeField] GameObject _highlightSquare;
    
    [Space(10)] [Header("Character")]
    [SerializeField] GameObject _characterInBlock;

    public void SetGrid(int x, int y)
    {
        _gridX = x;
        _gridY = y;
    }

    private void OnMouseUp()
    {
        if (!isClickable) return;
        GetComponentInParent<GridManager>().MapClick(_gridX, _gridY);
        //Debug.Log(_gridX + ":" + _gridY);
    }

    private void OnMouseEnter()
    {
        if (!isClickable) return;
        GetComponentInParent<GridManager>().MapHover(_gridX, _gridY);
    }

    public GameObject GetCharacterInBlock()
    {
        return _characterInBlock;
    }

    public void SetCharacterInBlock(GameObject character)
    {
        _characterInBlock = character;
    }
}
