using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageAnimatorControllerScript : AnimatorControllerScript
{
    public override void SetBlocking(bool blocking)
    {
        if (blocking)
        {
            _animator.SetTrigger("Spellcast_Raise");
        }
    }

    public override void SetHitBlock()
    {
        _animator.SetTrigger("Hit_Small");
    }
}
