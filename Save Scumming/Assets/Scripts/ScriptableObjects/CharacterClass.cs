using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character Class", menuName = "ScriptableObjects/Character Class")]

public class CharacterClass : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private int _movementRange;


    public string Name => _name;
    public int MovementRange => _movementRange;



}
