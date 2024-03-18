using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInfo : MonoBehaviour
{
    [SerializeField] private string _name;
    [SerializeField] private int _movementRange;

    [SerializeField] private int _damage;
    [SerializeField] private int _range;

    [SerializeField] private int _maxHp;
    [SerializeField] private int _hp;

    public string Name => _name;
    public int MovementRange => _movementRange;
    public int Damage => _damage;
    public int Range => _range;
    public int MaxHp => _maxHp;
    public int Hp => _hp;

    /// <summary>
    /// Change health value of Unit by an ammount
    /// Positive number to heal Unit, negative number to damage it
    /// </summary>
    /// <param name="value"></param>
    public void ModifyHp(int value)
    {
        _hp += value;
    }

    /// <summary>
    /// Change health value of Unit by a percentage of max health
    /// Positive number to heal Unit, negative number to damage it
    /// Percentage taken as a float value (ex. heal 20% of max Hp = 0.2f)
    /// </summary>
    /// <param name="value"></param>
    public void ModifyHpByPercent(float value)
    {
        _hp += Mathf.FloorToInt(_maxHp * value);
    }

    /// <summary>
    /// Change max health value of Unit by an ammount
    /// Positive number to increase max HP, negative number to decrease it
    /// </summary>
    /// <param name="value"></param>
    public void ModifyMaxHp(int value)
    {
        _maxHp += value;
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
    }
}
