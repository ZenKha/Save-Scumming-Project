using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitBehaviour : MonoBehaviour
{
    public bool _actionToken = false;
    public bool _moveToken = false;

    public bool MoveToken => _moveToken;
    public bool ActionToken => _actionToken;

    public void GiveTurnTokens()
    {
        _actionToken = true;
        _moveToken = true;
    }

    public void RemoveActionToken()
    {
        _actionToken = false;
        _moveToken = false;
    }

    public void RemoveMoveToken()
    {
        _moveToken = false;
    }
}
