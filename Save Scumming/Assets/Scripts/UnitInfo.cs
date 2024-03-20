using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInfo : MonoBehaviour
{
    [SerializeField] private string _name;
    [SerializeField] private int _movementRange;

    [SerializeField] private int _damage;
    [SerializeField] private int _attackRange;
    [SerializeField] private int _pierce;

    [SerializeField] private int _maxHp;
    [SerializeField] private int _hp;
    [SerializeField] private bool _isAlive = true;

    public string Name => _name;
    public int MovementRange => _movementRange;
    public int Damage => _damage;
    public int AttackRange => _attackRange;
    public int Pierce => _pierce;
    public int MaxHp => _maxHp;
    public int Hp => _hp;
    public bool IsAlive => _isAlive;

    /// <summary>
    /// Change health value of Unit by an ammount
    /// Positive number to heal Unit, negative number to damage it
    /// </summary>
    /// <param name="value"></param>
    public void ModifyHp(int value)
    {
        _hp += value;
        UpdateHP();
    }

    /// <summary>
    /// Change health value of Unit by a percentage of max health
    /// Positive number to heal Unit, negative number to damage it
    /// Percentage taken as a float value (ex. heal 20% of max Hp = 0.2f)
    /// </summary>
    /// <param name="value"></param>
    public void ModifyHpByPercentMaxHp(float value)
    {
        _hp += Mathf.FloorToInt(_maxHp * value);
        UpdateHP();
    }

    /// <summary>
    /// Change health value of Unit by a percentage of current health
    /// Positive number to heal Unit, negative number to damage it
    /// Percentage taken as a float value (ex. deal 50% of max Hp = -0.5f)
    /// </summary>
    /// <param name="value"></param>
    public void ModifyHpByPercentCurHp(float value)
    {
        _hp += Mathf.FloorToInt(_hp * value);
        UpdateHP();
    }

    /// <summary>
    /// Change max health value of Unit by an ammount
    /// Positive number to increase max HP, negative number to decrease it
    /// </summary>
    /// <param name="value"></param>
    public void ModifyMaxHp(int value)
    {
        _maxHp += value;
        UpdateHP();
    }

    /// <summary>
    /// Change health value of Unit by a percentage of max health
    /// Positive number to increase max HP, negative number to decrease it
    /// Percentage taken as a float value (ex. give bonus 50% of max Hp = 0.5f)
    /// </summary>
    /// <param name="value"></param>
    public void ModifyMaxHpByPercent(float value)
    {
        _maxHp += Mathf.FloorToInt(_maxHp * value);
        UpdateHP();
    }

    private void UpdateHP()
    {
        if (_hp > _maxHp) _hp = _maxHp;
        if (_hp < 0) _hp = 0;
        if (_maxHp > 99) _maxHp = 99;
        if (_maxHp < 0) _maxHp = 1;

        if (_hp == 0) _isAlive = false;
    }
}
