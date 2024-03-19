using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HighlightType { Close, Far, Attack}

public class Block : MonoBehaviour
{
    [SerializeField] bool _isClickable = false;
    public bool IsClickable => _isClickable;

    [Header("Coordinates")]
    [SerializeField] private int _gridX;
    [SerializeField] private int _gridY;

    public int GridX => _gridX;
    public int GridY => _gridY;

    [Space(10)] [Header("Character")]
    [SerializeField] GameObject _characterInBlock;

    public void SetGrid(int x, int y)
    {
        _gridX = x;
        _gridY = y;
    }

    private void OnMouseUp()
    {
        if (!_isClickable) return;
        GetComponentInParent<GridManager>().MapClick(_gridX, _gridY);
        //Debug.Log(_gridX + ":" + _gridY);
    }

    private void OnMouseEnter()
    {
        if (!_isClickable) return;
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

    [Space(10)]
    [Header("Highlight Tiles")]
    [SerializeField] GameObject _highlightSquare;
    [SerializeField] Material _highlightClose;
    [SerializeField] Material _highlightFar;
    [SerializeField] Material _highlightAttack;
    [SerializeField][Range(0f, 1f)] float HighlightYOffset; // 0.51f funciona por agora
    [SerializeField] private GameObject _highlightInstance;
    private bool _highlighted;
    public bool Highlighted => _highlighted;
    
    public void HighlightBlock(HighlightType highlightType)
    {
        if (!_isClickable)
        {
            return;
        }

        DestroyHighlight();

        Vector3 pos = transform.position;
        pos.y += HighlightYOffset;
        _highlightInstance = Instantiate(_highlightSquare, pos, Quaternion.identity);

        switch (highlightType)
        {
            case HighlightType.Close:
                _highlightInstance.GetComponent<MeshRenderer>().material = _highlightClose;
                break;
            case HighlightType.Far:
                _highlightInstance.GetComponent<MeshRenderer>().material = _highlightFar;
                break;
            case HighlightType.Attack:
                _highlightInstance.GetComponent<MeshRenderer>().material = _highlightAttack;
                break;
        }

        _highlighted = true;
    }

    public void DestroyHighlight()
    {
        if (!_isClickable)
        {
            return;
        }

        Destroy(_highlightInstance);
        _highlightInstance = null;
        _highlighted = false;
    }
}
