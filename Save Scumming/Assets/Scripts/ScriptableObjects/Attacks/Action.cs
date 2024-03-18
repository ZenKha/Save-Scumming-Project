using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetType { SELF, MELEE, LINE, AOE, TARGETED }

//[CreateAssetMenu(fileName = "New Action", menuName = "ScriptableObjects/Action")]
public class Action : ScriptableObject
{
    [Header("Action Stats")]
    [SerializeField] private string _name;
    [SerializeField] private int _damage;
    [SerializeField] private int _range;
    [SerializeField] private List<TargetType> _targets;

    
}
