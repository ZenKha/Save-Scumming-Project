using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorControllerScript : MonoBehaviour
{
    [SerializeField] protected Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public void SetMoving(bool moving)
    {
        _animator.SetBool("Moving", moving);
    }

    public virtual void SetBlocking(bool blocking)
    {
        _animator.SetBool("Blocking", blocking);
    }

    public void SetResting(bool resting)
    {
        _animator.SetBool("Resting", resting);
    }

    public void SetDeath()
    {
        _animator.SetTrigger("Death");
    }

    public void SetVictory()
    {
        _animator.SetTrigger("Victory");
    }

    public void SetAttack()
    {
        _animator.SetTrigger("Attack");
    }

    public void SetHitSmall()
    {
        _animator.SetTrigger("Hit_Small");
    }

    public void SetHitBig()
    {
        _animator.SetTrigger("Hit_Big");
    }

    public virtual void SetHitBlock()
    {
        _animator.SetTrigger("Block_Hit");
    }
}
