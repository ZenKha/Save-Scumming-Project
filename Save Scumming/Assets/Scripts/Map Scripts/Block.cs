using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HighlightType { Close, Far, Attack, AttackHover}

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
        GridManager.instance.MapClick(_gridX, _gridY);
    }

    private void OnMouseEnter()
    {
        if (!_isClickable) return;
        GridManager.instance.MapHover(_gridX, _gridY);
    }

    private void OnMouseExit()
    {
        if (!_isClickable) return;
        GridManager.instance.MapHover(-1, -1);
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
    [SerializeField] Material _highlightAttackHover;
    [SerializeField][Range(0f, 1f)] float HighlightYOffset; // 0.51f funciona por agora
    [SerializeField] private GameObject _highlightInstance;
    private bool _isHighlighted;
    public bool IsHighlighted => _isHighlighted;
    
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
            case HighlightType.AttackHover:
                _highlightInstance.GetComponent<MeshRenderer>().material = _highlightAttackHover;
                break;
        }

        _isHighlighted = true;
    }

    public void DestroyHighlight()
    {
        if (!_isClickable)
        {
            return;
        }

        Destroy(_highlightInstance);
        _highlightInstance = null;
        _isHighlighted = false;
    }
}
